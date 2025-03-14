using BaboonAPI.Hooks.Entrypoints;
using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using TootTallyCore.Graphics;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.Assets;
using TootTallyCore.Utils.TootTallyGlobals;
using TootTallyCore.Utils.TootTallyModules;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallyCore.Utils.Steam;
using UnityEngine;
using TootTallyCore.Utils.Helpers;

namespace TootTallyCore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("TootTally")]
    public class Plugin : BaseUnityPlugin
    {
        public static int BUILDDATE = 20240331;
        private const string DEFAULT_THEME = "Default";

        public static Plugin Instance;
        private Harmony _harmony;

        public static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        public static void LogWarning(string msg) => Instance.Logger.LogWarning(msg);
        public static void LogError(string msg) => Instance.Logger.LogError(msg);
        public static void LogDebug(string msg)
        {
            if (!Instance.DebugMode.Value) return;
            Instance.Logger.LogDebug(msg);
        }
        public static void LogException(Exception ex)
        {
            LogError(ex.Message);
            LogError(ex.StackTrace);
        }

        public ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<bool> ShouldShowNotifs { get; private set; }
        public ConfigEntry<string> ThemeName { get; private set; }
        public ConfigEntry<bool> RunGCWhilePlaying { get; private set; }
        public ConfigEntry<bool> ChangePitch { get; private set; }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            ShouldShowNotifs = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            DebugMode = Config.Bind("General", "Debug Mode", false, "Add extra logging information for debugging.");
            ThemeName = Config.Bind("Themes", "ThemeName", DEFAULT_THEME.ToString());
            RunGCWhilePlaying = Config.Bind("General", "Deactivate Garbage Collector While Playing.", false, "Deactivate the garbage collector during gameplay to prevent lag spikes.");
            ChangePitch = Config.Bind("General", "Change Pitch", false, "Change the pitch on speed changes.");
            Config.SettingChanged += ThemeManager.Config_SettingChanged;

            string targetThemePath = Path.Combine(Paths.BepInExRootPath, "Themes");
            string sourceThemePath = Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Themes");
            FileHelper.TryMigrateFolder(sourceThemePath, targetThemePath);

            _harmony = new Harmony(Info.Metadata.GUID);
            GameInitializationEvent.Register(Info, TryInitialize);
        }


        private void Update()
        {
            if (!_isReloadingTracks && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                _isReloadingTracks = true;
                TootTallyNotifManager.DisplayNotif("Reloading tracks... Lag is normal.");
                Plugin.Instance.Invoke("ReloadTracks", .5f);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
            {
                TootTallyNotifManager.DisplayNotif("Forcing garbage collection.");
                for (int i = 0; i < 9; i++)
                    GC.Collect(i % 3, GCCollectionMode.Forced);
            }
        }

        private bool _isReloadingTracks;

        public void ReloadTracks()
        {
            TrackLookup.reload();
            _isReloadingTracks = false;
        }

        private void TryInitialize()
        {
            AssetManager.LoadAssets(Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets"));

            _harmony.PatchAll(typeof(ThemeManager));
            _harmony.PatchAll(typeof(GameObjectFactory));
            _harmony.PatchAll(typeof(TootTallyPatches));
            gameObject.AddComponent<TootTallyNotifManager>();
            gameObject.AddComponent<TootTallyAnimationManager>();

            TootTallyModuleManager.LoadModules();
            SteamAuthTicketHandler.GetSteamAuthTicket();

            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {Application.version}");
        }
    }
}
