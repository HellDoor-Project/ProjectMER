using HarmonyLib;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectMER.Features.Serializable.Schematics.CustomScripts
{
    public class CustomScriptAttacher
    {
        public static void AttachScript(SchematicObjectDataList data, SchematicObject schematic)
        {
            try
            {
                Type scriptType = AccessTools.TypeByName(data.ScriptClassName);
                if (!scriptType.IsSubclassOf(typeof(CustomScript))) return;
                MethodInfo addComponent = AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent));
                object result = addComponent.MakeGenericMethod(scriptType).Invoke(schematic.gameObject, []);
                ((CustomScript)result).Init(schematic);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding script {data.ScriptClassName}");
                Logger.Error(ex);
            }
        }
    }
}
