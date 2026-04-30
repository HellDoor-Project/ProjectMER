using System.Globalization;
using CommandSystem.Commands.Shared;
using LabApi.Features.Wrappers;
using MEC;
using ProjectMER.Features.Actions;
using ProjectMER.Features.Extensions;
using RemoteAdmin;
using UnityEngine;

namespace ProjectMER.Features.Objects;

public class ActionEventHostObject
{
	public ActionEventHostObject(SchematicObject schematic, int hostObjectId)
	{
		_schematic = schematic;
		_hostObjectId = hostObjectId;
	}

	public IReadOnlyList<ActionEventList> ActionEvents => _actionEvents;

	public Dictionary<string, List<ActionGame>> ActionsByEventId { get; } = new(StringComparer.Ordinal);

	public CoroutineHandle RunActions(string eventId, Player? target = null) => Timing.RunCoroutine(ExecuteActionsCoroutine(eventId, target));
	
	public bool TryGetActions(string eventId, out List<ActionGame> actions) =>
		ActionsByEventId.TryGetValue(eventId, out actions);

	public IEnumerator<float> ExecuteActionsCoroutine(string eventId, Player? target = null)
	{
		if (string.IsNullOrWhiteSpace(eventId))
			yield break;

		if (!TryGetActions(eventId, out List<ActionGame> actions) || actions.Count == 0)
			yield break;
		
		foreach (ActionGame action in actions)
		{
			if (action == null)
			{
				yield return Timing.WaitForOneFrame;
				continue;
			}

			if (action.ActionDelay > 0f)
			{
				yield return Timing.WaitForSeconds(action.ActionDelay);
			}

			try
			{
				ExecuteAction(action, target);
			}
			catch (Exception e)
			{
				Logger.Error($"Action execution failed for EventId '{eventId}' and action type '{action.Type}': {e}");
			}
			yield return Timing.WaitForOneFrame;
		}
	}

	internal void SetActionEvents(List<ActionEventList> actionEvents)
	{
		_actionEvents = actionEvents ?? [];
		ActionsByEventId.Clear();
		_animatorByObjectIdCache.Clear();
		_animParamHashCache.Clear();

		ActionsByEventId.AddRange(ActionEventSerialization.BuildEventDictionary(_actionEvents));
	}

	private void ExecuteAction(ActionGame action, Player? target)
	{
		switch (action.Type)
		{
			case ActionType.Command:
				ExecuteCommandAction(action, target);
				break;
			case ActionType.Animation:
				ExecuteAnimationAction(action);
				break;
			case ActionType.Audio:
				ExecuteAudioAction(action);
				break;
			default:
				Logger.Warn($"Unknown action type: {action.Type}");
				break;
		}
	}

	private static void ExecuteCommandAction(ActionGame action, Player? target)
	{
		if (string.IsNullOrWhiteSpace(action.Value))
			return;
		
		var args = action.Value.Trim().Split(' ', 512, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length == 0)
			return;
		if (args[0].StartsWith('!'))
		{
			Logger.Warn("Command starts with '!'. Not allowed!");
			return;
		}
		if (!CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(args[0], out var command))
		{
			Logger.Warn($"Command execution failed for command {args[0]}. Command not found!");
			return;
		}
		if (ProjectMER.Singleton.Config.BannedCommandsForActions.Contains(command.Command.ToLower()))
		{
			Logger.Warn($"Command '{command.Command}' banned for Actions");
			return;
		}
		
		if (target != null)
		{
			for (var i = 0; i < args.Length; i++)
			{
				if (args[i].ToLower() is "[p_id]" or "{p_id}")
				{
					args[i] = target.PlayerId.ToString();
				}
			}
		}

		// Logger.Info($"Trying to execute {string.Join(' ', args)}");
		command.Execute(args.Segment(1), ServerConsole.Scs, out _);
	}

	private void ExecuteAnimationAction(ActionGame action)
	{
		if (string.IsNullOrWhiteSpace(action.Param))
			return;

		int targetObjectId = action.TargetId != 0 ? action.TargetId : _hostObjectId;
		Animator? animator = ResolveAnimator(targetObjectId);
		if (animator == null)
		{
			Logger.Warn($"Animation action skipped: no Animator for object id {targetObjectId}.");
			return;
		}

		int paramHash = GetAnimatorParamHash(action.Param);

		switch (action.ParamType)
		{
			case AnimatorControllerParameterType.Trigger:
				animator.SetTrigger(paramHash);
				break;

			case AnimatorControllerParameterType.Bool:
				animator.SetBool(paramHash, ParseBool(action.Value));
				break;

			case AnimatorControllerParameterType.Int:
				animator.SetInteger(paramHash, ParseInt(action.Value));
				break;

			case AnimatorControllerParameterType.Float:
				animator.SetFloat(paramHash, ParseFloat(action.Value));
				break;

			default:
				Logger.Warn($"Animation action skipped: unsupported ParamType {action.ParamType}.");
				break;
		}
	}

	private void ExecuteAudioAction(ActionGame action)
	{
		Logger.Warn($"Audio action is not implemented for runtime yet. Value: '{action.Value}'.");
	}

	private Animator? ResolveAnimator(int targetObjectId)
	{
		if (_animatorByObjectIdCache.TryGetValue(targetObjectId, out Animator cachedAnimator))
		{
			if (cachedAnimator != null)
				return cachedAnimator;

			_animatorByObjectIdCache.Remove(targetObjectId);
		}

		if (!_schematic.ObjectFromId.TryGetValue(targetObjectId, out Transform targetTransform) || targetTransform == null)
			return null;

		GameObject target = targetTransform.gameObject;
		Animator? targetAnimator = target.GetComponent<Animator>();
		if (targetAnimator != null)
			_animatorByObjectIdCache[targetObjectId] = targetAnimator;

		return targetAnimator;
	}

	private int GetAnimatorParamHash(string paramName)
	{
		if (_animParamHashCache.TryGetValue(paramName, out int cachedHash))
			return cachedHash;

		int hash = Animator.StringToHash(paramName);
		_animParamHashCache[paramName] = hash;
		return hash;
	}

	private static bool ParseBool(string value)
	{
		if (bool.TryParse(value, out bool boolValue))
			return boolValue;

		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
			return intValue != 0;

		return false;
	}

	private static int ParseInt(string value)
	{
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
			return intValue;

		return 0;
	}

	private static float ParseFloat(string value)
	{
		if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
			return floatValue;

		return 0f;
	}

	private List<ActionEventList> _actionEvents = [];
	private readonly SchematicObject _schematic;
	private readonly int _hostObjectId;
	private readonly Dictionary<int, Animator> _animatorByObjectIdCache = [];
	private readonly Dictionary<string, int> _animParamHashCache = new(StringComparer.Ordinal);
}
