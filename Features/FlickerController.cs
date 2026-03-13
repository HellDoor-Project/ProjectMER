using AdminToys;
using UnityEngine;

namespace ProjectMER.Features;

public class FlickerController : MonoBehaviour
{
    public static readonly List<FlickerController> Instances = new();
    public LightSourceToy LightSourceToy;
    private float _prevLightRange;
    private float _flickerDuration;
    private bool _lightEnabled = true;

    public void Start()
    {
        LightSourceToy = GetComponent<LightSourceToy>();
        if (LightSourceToy == null)
        {
            Logger.Error("FlickerController: No LightSourceToy found! Destroying FlickerController!");
            Destroy(this);
            return;
        }
        Instances.Add(this);
    }

    public void OnDestroy()
    {
        Instances.Remove(this);
    }

    public void Update()
    {
        if (_flickerDuration <= 0.0)
            return;
        _flickerDuration -= Time.deltaTime;
        if (_flickerDuration > 0.0)
            return;
        SetLights(true);
    }

    public void ServerFlickerLights(float dur)
    {
        if (dur <= 0.0)
        {
            _flickerDuration = 0.0f;
            SetLights(true);
        }
        else
        {
            _flickerDuration = dur;
            SetLights(false);
        }
    }
    
    public void SetLights(bool state)
    {
        if (state)
        {
            if (_lightEnabled)
                return;
            _lightEnabled = true;
            LightSourceToy?.NetworkLightRange = _prevLightRange;
        }
        else
        {
            if (!_lightEnabled)
                return;
            _lightEnabled = false;
            _prevLightRange = LightSourceToy.NetworkLightRange;
            LightSourceToy?.NetworkLightRange = 0.0f;
        }
    }
}