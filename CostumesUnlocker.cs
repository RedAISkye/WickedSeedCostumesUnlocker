using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace RedAISkye.WickedSeed.CostumesUnlocker
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class CostumesUnlocker : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(PlayerData), "GetOwnedCostumes")]
    public static class PlayerData_GetOwnedCostumes_Patch
    {
        private static bool costumeNamelogged = false;
        public static bool Prefix(ref List<Costume> __result, PlayerData __instance)
        {
            FieldInfo dbField = typeof(PlayerData).GetField("dbCostumes", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dbField == null)
            {
                CostumesUnlocker.Logger.LogError("dbCostumes field not found in PlayerData!");
                return true;
            }

            var dbCostumes = dbField.GetValue(__instance);
            MethodInfo getAllMethod = dbCostumes.GetType().GetMethod("GetAll");
            __result = (List<Costume>)getAllMethod.Invoke(dbCostumes, null);

            if (!costumeNamelogged)
            {
                costumeNamelogged = true;

                string allCostumes = string.Join(", ", __result.ConvertAll(c => c.name));
                CostumesUnlocker.Logger.LogInfo($"Total Costumes: {__result.Count}");
                CostumesUnlocker.Logger.LogInfo($"Unlocked Costumes: {allCostumes}");
            }

            return false;
        }
    }
}
