using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectMER.Features.Serializable.Schematics.CustomScripts
{
    public abstract class CustomScript : MonoBehaviour
    {
        public abstract void Init(SchematicObject schematic);
    }
}
