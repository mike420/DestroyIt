using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;

namespace DestroyIt;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.None)]
public class DestroyItMod : BaseUnityPlugin
{
    public const string PluginName = "DestroyIt";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "MagicMike";
    internal const string PluginGUID = "MagicMike.DestroyIt";
    internal const string PluginCopy = "Copyright © MagicMike 2026";
    public const string PluginLink = "https://discord.gg/mnr36G6J4x";

    private Harmony harmony;

    public void Awake()
    {
        MainConfig.Init(Config);
        MagicMike.ModSettings.ModSettings.Init(this);

        if (!MainConfig.modEnabled.Value)
        {
            return;
        }

        Translations.Load(this);
        harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGUID);
    }

    public void Update()
    {
        if (MainConfig.modEnabled.Value && MainConfig.demolishKey.Value.IsDown() && !Demolish.InputBlocked())
        {
            Demolish.OnKey();
        }
    }

    public void OnDestroy()
    {
        harmony?.UnpatchSelf();
    }
}
