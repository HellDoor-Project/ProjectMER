using HarmonyLib;
using ProjectMER.Features.Objects;

namespace ProjectMER.Features.Serializable.Schematics.CustomScripts
{
    public class CustomScriptAttacher
    {
        public static void AttachScript(SchematicObjectDataList data, SchematicObject schematic)
        {
            try
            {
                Type scriptType = AccessTools.TypeByName(data.ScriptClassName);
                if (!scriptType.IsSubclassOf(typeof(CustomScript)))
                {
                    Logger.Warn($"Script class \"{data.ScriptClassName}\" is not a subclass of CustomScript");
                    return;
                }
                var component = schematic.gameObject.AddComponent(scriptType);
                ((CustomScript)component).Init(schematic);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding script {data.ScriptClassName}");
                Logger.Error(ex);
            }
        }
    }
}
