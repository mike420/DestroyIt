using HarmonyLib;

namespace DestroyIt;

internal static class StationPatches
{
    // Player.CheckCanRemovePiece is the game's rule that removing a piece with the hammer needs its crafting station
    // (the workbench and so on) nearby. It is private, so it is patched by name. With Require Crafting Station off,
    // the rule always passes, so the hammer's remove tool and its hover check no longer ask for the station.
    [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
    public static class CheckCanRemovePiece_Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (MainConfig.requireCraftingStation.Value)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
