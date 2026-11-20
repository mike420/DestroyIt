using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Jotunn.Extensions;
using UnityEngine;

namespace DestroyIt;

internal enum AllowDestroy
{
    Disabled,
    OnlyBoats,
    OnlyCarts,
    Both
}

internal static class MainConfig
{
    public static ConfigEntry<bool> modEnabled;
    public static ConfigEntry<AllowDestroy> allowDestroyFor;
    public static ConfigEntry<bool> requireEmptyContainer;
    public static ConfigEntry<bool> allowDestroySiegeWeapons;
    public static ConfigEntry<bool> requireCraftingStation;
    public static ConfigEntry<KeyboardShortcut> demolishKey;
    public static ConfigEntry<bool> demolishConfirm;
    public static ConfigEntry<float> demolishRadius;

    public static void Init(ConfigFile config)
    {
        modEnabled = config.BindConfigInOrder("General Settings", "Enable Plugin", defaultValue: true, "Enable the DestroyIt plugin.\n(Restart required. It is read before the game starts, so it is a local setting).\n", synced: false, sectionOrder: true, settingOrder: true);
        requireCraftingStation = config.BindConfigInOrder("General Settings", "Require Crafting Station", defaultValue: false, "If On, a piece can only be destroyed (with the hammer or the Demolish key) when its crafting station, such as the workbench, is near you. That is the game's own rule. If Off, pieces can be destroyed wherever you are.", synced: false, sectionOrder: true, settingOrder: true);

        allowDestroyFor = config.BindConfigInOrder("Destroy", "Allow Destroy For", defaultValue: AllowDestroy.Both, "Which of these you can destroy straight away with the hammer's remove tool, as long as Require Empty Container and the game's own checks allow it.", synced: false, sectionOrder: true, settingOrder: true);
        requireEmptyContainer = config.BindConfigInOrder("Destroy", "Require Empty Container", defaultValue: true, "If On, the boat or cart's chest must be empty before it can be destroyed. (A private chest must always be empty).", synced: false, sectionOrder: true, settingOrder: true);
        allowDestroySiegeWeapons = config.BindConfigInOrder("Destroy", "Allow Destroy Siege Weapons", defaultValue: true, "If On, you can destroy battering rams and catapults straight away with the hammer's remove tool, as long as nobody is using them.", synced: false, sectionOrder: true, settingOrder: true);

        demolishKey = config.BindConfigInOrder("Demolish", "Key", defaultValue: new KeyboardShortcut(KeyCode.Semicolon), "Press this to destroy everything around you within the Radius, like removing each piece with the hammer. Any key, or a key with modifiers such as \"Semicolon + LeftAlt\".", synced: false, sectionOrder: true, settingOrder: true);
        demolishConfirm = config.BindConfigInOrder("Demolish", "Require Confirmation", defaultValue: true, "If On, the first press of the Key only shows how many pieces would be destroyed, and you press it again within 5 seconds to confirm.", synced: false, sectionOrder: true, settingOrder: true);
        demolishRadius = config.BindConfigInOrder("Demolish", "Radius", defaultValue: 20f, "How far around you, in metres, the Key destroys.", synced: false, sectionOrder: true, settingOrder: true);

        CarryOverOldSections(config);
    }

    private static readonly Dictionary<string, string[]> OldSections = new Dictionary<string, string[]>
    {
        { "General Settings", new[] { "Demolish", "Destroy" } },   // Require Crafting Station used to be in the Demolish, then the Destroy, section
    };

    private static void CarryOverOldSections(ConfigFile config)
    {
        Dictionary<ConfigDefinition, string> orphans;
        try
        {
            PropertyInfo property = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            orphans = property?.GetValue(config) as Dictionary<ConfigDefinition, string>;
        }
        catch (System.Exception e)
        {
            Jotunn.Logger.LogWarning("[DestroyIt] Could not look for settings under old section names: " + e.Message);
            return;
        }
        if (orphans == null || orphans.Count == 0)
        {
            return;
        }

        int carried = 0, dropped = 0;
        HashSet<ConfigEntryBase> handled = new HashSet<ConfigEntryBase>();
        bool saveOnSet = config.SaveOnConfigSet;
        config.SaveOnConfigSet = false;
        try
        {
            // An earlier renumbering can leave the same setting under several old numbers; the highest is the latest
            foreach (ConfigDefinition old in orphans.Keys.OrderByDescending(d => SectionNumber(d.Section)).ToList())
            {
                string oldName = SectionName(old.Section);
                ConfigEntryBase entry = ((IDictionary<ConfigDefinition, ConfigEntryBase>)config).Values.FirstOrDefault(e => e.Definition.Key == old.Key && e.Definition.Section != old.Section
                    && (SectionName(e.Definition.Section) == oldName || OldSections.TryGetValue(SectionName(e.Definition.Section), out string[] before) && before.Contains(oldName)));
                if (entry == null)
                {
                    continue;   // (not one of ours any more: left as it is)
                }
                if (handled.Add(entry) && Equals(entry.BoxedValue, entry.DefaultValue))
                {
                    try
                    {
                        entry.SetSerializedValue(orphans[old]);
                        carried++;
                    }
                    catch (System.Exception e)
                    {
                        Jotunn.Logger.LogWarning("[DestroyIt] Could not carry over " + old.Section + " / " + old.Key + ": " + e.Message);
                    }
                }
                orphans.Remove(old);
                dropped++;
            }
        }
        finally
        {
            config.SaveOnConfigSet = saveOnSet;
        }
        if (dropped > 0)
        {
            config.Save();
            Jotunn.Logger.LogInfo("[DestroyIt] Carried " + carried + " setting(s) over from old section names in the config file.");
        }
    }

    // A section's name without the number in front ("4 - Fortified Ballista": Fortified Ballista)
    private static string SectionName(string section) => Regex.Replace(section ?? "", @"^\d+\s*-\s*", "");

    private static int SectionNumber(string section)
    {
        Match match = Regex.Match(section ?? "", @"^(\d+)\s*-");
        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }
}
