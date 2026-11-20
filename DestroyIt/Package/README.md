# 🔨 DestroyIt

Destroy boats and carts straight away with the hammer's remove tool. The base game already knows when a boat or cart could be removed, but switches it off, so everyone ends up taking an axe to them. This turns it back on.

<br>

## 💶 Donations: 

If you enjoy using this Valheim mod, please consider making a small donation to help support its development and future updates.

Even £1 or $1 would be greatly appreciated and would help more than you might think!

Thank you for your support, and happy gaming! ⚔️  ...... (A Paypal account is NOT required!)

<a href="https://www.paypal.com/donate/?hosted_button_id=BN8HEWHRMAREU">![](https://imgup.uk/i/n7Sg7WK9.png)</a>

<br>

## ✅ Features:

  * Destroy **boats** with the hammer when nobody is on board.
  * Destroy **carts** with the hammer when nobody is pulling them.
  * Optionally only when the chest is empty. A private chest must always be empty.
  * Choose boats, carts, both, or neither, from the in-game settings panel (F7).
  * Destroy **battering rams** and **catapults** with the hammer when nobody is using them.
  * **Demolish**: press a key (`;` by default) to destroy everything you built within a radius around you. It first shows how many pieces it would destroy, and you press the key again to confirm. It follows the hammer's rules (your own pieces, no-build zones, wards) and the boat and cart settings.
  * Skips ValheimRaft boats, which handle their own removal.
  * Client side: it does not need to be on the server.

<br>

## ⚙️ Settings:

| Setting | Default | What it does |
|---|---|---|
| `Enable Plugin` | On | Turns the plugin on. Needs a restart. |
| `Require Crafting Station` | Off | If On, a piece can only be destroyed, with the hammer or the Demolish key, when its crafting station (such as the workbench) is near you. That is the game's own rule. If Off, you can destroy pieces wherever you are. |
| `Allow Destroy For` | Both | Boats, carts, both or neither. |
| `Require Empty Container` | On | The chest must be empty before it can be destroyed. |
| `Allow Destroy Siege Weapons` | On | Battering rams and catapults can be destroyed with the hammer. |
| `Key` (Demolish) | `;` | Destroys everything you built within the Radius. Any key, or a key with modifiers. |
| `Require Confirmation` (Demolish) | On | The first press shows how many pieces, and you press the key again within 5 seconds to confirm. |
| `Radius` (Demolish) | 20 | How far around you, in metres, the Key destroys. |

<br>

## 🌍 Translations:

The translation files are in `BepInEx/config/DestroyIt/Translations/<Language>/DestroyIt.json`. Edit them, or add a language, and restart the game.

<br>

## 🙏 Credits:

  * Based on <a href="https://github.com/Goldenrevolver/ValheimMiniMods/tree/main/InstantlyDestroyBoatsAndCarts">Instantly Destroy Boats and Carts</a> by Goldenrevolver.
