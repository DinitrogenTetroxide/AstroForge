using HarmonyLib;
using SFS.Parts;

namespace LuaInterpreter
{
    public class Patcher
    {
        // I'm a complete noob at Harmony patches
        [HarmonyPatch(typeof(CustomAssetsLoader), "LoadTexturePacks")]
        public static class LoadTexturesPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                Main.loader.LoadCustomParts();
            }
        }
    }
}
