using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DestroyIt;

// Destroys the pieces around the player in one go. The same idea as Building Demolish by aedenthorn
// (github.com/aedenthorn/ValheimMods/tree/master/BuildingDemolish), written against the current game: each piece goes
// through the same checks and removal steps as Player.RemovePiece (the hammer's remove tool).
internal static class Demolish
{
    // The layers the hammer's remove tool looks at (Player.m_removeRayMask)
    private static readonly int Mask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "vehicle");

    // How long after the first press the second press still confirms
    private const float ConfirmSeconds = 5f;

    private static float confirmUntil;

    // The Key was pressed: ask first (if Require Confirmation is on), then destroy
    public static void OnKey()
    {
        float radius = MainConfig.demolishRadius.Value;

        if (MainConfig.demolishConfirm.Value && Time.time > confirmUntil)
        {
            int pending = Pieces(radius, dryRun: true);
            if (pending == 0)
            {
                Say(MessageHud.MessageType.TopLeft, Translations.T("destroyit_nothing_to_demolish"));
                return;
            }

            confirmUntil = Time.time + ConfirmSeconds;
            Say(MessageHud.MessageType.Center, Translations.T("destroyit_confirm", pending.ToString(CultureInfo.CurrentCulture), MainConfig.demolishKey.Value.ToString()));
            return;
        }

        confirmUntil = 0f;
        int count = Pieces(radius, dryRun: false);
        Say(MessageHud.MessageType.TopLeft, count == 0 ? Translations.T("destroyit_nothing_to_demolish") : Translations.T("destroyit_demolished", count.ToString(CultureInfo.CurrentCulture)));
    }

    // Destroys the pieces within the radius and returns how many. With dryRun it only counts them.
    public static int Pieces(float radius, bool dryRun = false)
    {
        Player player = Player.m_localPlayer;
        if (!player)
        {
            return 0;
        }

        int count = 0;
        // one piece has several colliders: each piece is only handled once
        HashSet<Piece> done = new HashSet<Piece>();
        foreach (Collider collider in Physics.OverlapSphere(player.transform.position, radius, Mask))
        {
            Piece piece = collider.GetComponentInParent<Piece>();
            if (!piece || !done.Add(piece) || !CanRemove(player, piece))
            {
                continue;
            }

            if (!dryRun)
            {
                Remove(player, piece);
            }
            count++;
        }
        return count;
    }

    private static bool CanRemove(Player player, Piece piece)
    {
        // only your own pieces
        if (!piece.IsCreator())
        {
            return false;
        }
        if (!piece.m_canBeRemoved)
        {
            return false;
        }
        if (Location.IsInsideNoBuildLocation(piece.transform.position))
        {
            return false;
        }
        if (!PrivateArea.CheckAccess(piece.transform.position, 0f, flash: false))
        {
            return false;
        }
        if (MainConfig.requireCraftingStation.Value && !HaveStation(player, piece))
        {
            return false;
        }
        if (!piece.GetComponent<ZNetView>())
        {
            return false;
        }
        return piece.CanBeRemoved();
    }

    private static void Remove(Player player, Piece piece)
    {
        piece.GetComponent<IRemoved>()?.OnRemoved();
        WearNTear wearNTear = piece.GetComponent<WearNTear>();
        if (wearNTear)
        {
            wearNTear.Remove();
            return;
        }

        Character character = piece.GetComponent<Character>();
        if (character)
        {
            character.Damage(new HitData(1E+10f));
            return;
        }

        piece.GetComponent<ZNetView>().ClaimOwnership();
        piece.DropResources();
        piece.m_placeEffect.Create(piece.transform.position, piece.transform.rotation, piece.gameObject.transform, 1f, -1, player.GetZDOID());
        player.m_removeEffects.Create(piece.transform.position, Quaternion.identity, null, 1f, -1, player.GetZDOID());
        ZNetScene.instance.Destroy(piece.gameObject);
    }

    // Player.CheckCanRemovePiece, which is private and shows a message for each piece it refuses
    private static bool HaveStation(Player player, Piece piece)
    {
        return player.PlacementCostDisabled
            || piece.m_craftingStation == null
            || CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, player.transform.position)
            || ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench);
    }

    // The keys that must not trigger it: typing, menus and the like (the same list the original mod uses)
    public static bool InputBlocked()
    {
        return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible()
            || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || InventoryGui.IsVisible()
            || Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
    }

    private static void Say(MessageHud.MessageType type, string text)
    {
        Player.m_localPlayer?.Message(type, text);
    }
}
