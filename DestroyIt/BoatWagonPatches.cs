using HarmonyLib;
using UnityEngine;

namespace DestroyIt;

// Based on InstantlyDestroyBoatsAndCarts by Goldenrevolver. The base game already has the logic for when a boat or cart
// may be removed with the hammer, but switches it off; this turns it back on.
internal static class BoatWagonPatches
{
    [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
    public static class Awake_Patch
    {
        private static void Postfix(Piece __instance)
        {
            Ship ship = __instance.GetComponentInChildren<Ship>();
            Vagon vagon = __instance.GetComponentInChildren<Vagon>();

            if (vagon || GetSiegeWeapon(__instance))
            {
                __instance.m_canBeRemoved = true;
            }

            if (ship && !IsValheimRaft(ship))
            {
                __instance.m_canBeRemoved = true;
            }
        }
    }

    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    public static class CanBeRemoved_Patch
    {
        private static bool Prefix(Piece __instance, ref bool __result)
        {
            Ship ship = __instance.GetComponentInChildren<Ship>();
            Vagon cart = __instance.GetComponentInChildren<Vagon>();
            Component siege = GetSiegeWeapon(__instance);

            if (!ship && !cart && !siege)
            {
                return true;
            }

            // Battering rams and catapults: their own setting, handled before the cart rules
            if (siege && !ship)
            {
                __result = false;

                if (MainConfig.allowDestroySiegeWeapons.Value)
                {
                    // both prefabs carry a Vagon on the same root object as the Piece
                    __result = !cart || !cart.InUse();
                }
                return false;
            }

            if (ship && IsValheimRaft(ship))
            {
                return true;
            }

            __result = false;

            AllowDestroy allow = MainConfig.allowDestroyFor.Value;

            if (ship && (allow == AllowDestroy.OnlyBoats || allow == AllowDestroy.Both))
            {
                if (CanRemoveContainer(__instance.GetComponentInChildren<Container>()))
                {
                    // the base game's own check: nobody on board (Ship.m_players is private at runtime)
                    __result = ship.CanBeRemoved();
                }
                return false;
            }

            if (cart && (allow == AllowDestroy.OnlyCarts || allow == AllowDestroy.Both))
            {
                if (CanRemoveContainer(cart.m_container))
                {
                    __result = !cart.InUse();
                }
                return false;
            }

            return false;
        }
    }

    // The battering ram (SiegeMachine) or catapult on this piece, if any
    private static Component GetSiegeWeapon(Piece piece)
    {
        Component catapult = piece.GetComponentInChildren<Catapult>();
        return catapult ? catapult : piece.GetComponentInChildren<SiegeMachine>();
    }

    // Container.CanRemove, with a null check and the Require Empty Container setting
    private static bool CanRemoveContainer(Container container)
    {
        if (!container)
        {
            return true;
        }

        bool isPrivate = container.m_privacy == Container.PrivacySetting.Private;
        return !((isPrivate || MainConfig.requireEmptyContainer.Value) && container.GetInventory().NrOfItems() > 0);
    }

    // ValheimRaft boats handle their own removal
    private static bool IsValheimRaft(Ship ship)
    {
        if (ship.name.StartsWith("MBRaft"))
        {
            return true;
        }

        foreach (Component component in ship.GetComponents<Component>())
        {
            if (component.GetType().Name == "MoveableBaseShipComponent")
            {
                return true;
            }
        }

        return false;
    }
}
