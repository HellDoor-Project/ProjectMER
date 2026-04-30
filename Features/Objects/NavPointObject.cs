using UnityEngine;

namespace ProjectMER.Features.Objects;

public class NavPointObject : MonoBehaviour
{
    public List<NavPointObject> LinkNavPoints = new List<NavPointObject>();
    public float Radius;
    public bool ForceJump;
    public bool AllowShortcut;
}