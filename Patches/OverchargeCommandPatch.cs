using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using MapGeneration;
using ProjectMER.Features;

namespace ProjectMER.Patches;

[HarmonyPatch(typeof(OverchargeCommand), nameof(OverchargeCommand.Overcharge))]
public static class OverchargeCommandPatch
{
    public static bool Prefix(FacilityZone zoneToAffect, float duration)
    {
        if (zoneToAffect != FacilityZone.None)
            return true;

        foreach (var flicker in FlickerController.Instances)
        {
            flicker.ServerFlickerLights(duration);
        }
        return true;
    }
}