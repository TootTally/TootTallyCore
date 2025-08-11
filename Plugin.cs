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
        public static int BUILDDATE = 20250701;
        private const string DEFAULT_THEME = "Default";

        public static Plugin Instance;
        private Harmony _harmony;

        public readonly ReloadManager reloadManager;

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
        public ConfigEntry<float> OffsetAtDefaultSpeed { get; private set; }
        public ConfigEntry<bool> CompatibilityHasConvertedOffsetCheck { get; private set; }

        public Plugin()
        {
            reloadManager = new ReloadManager(this);
        }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            ShouldShowNotifs = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            DebugMode = Config.Bind("General", "Debug Mode", false, "Add extra logging information for debugging.");
            ThemeName = Config.Bind("Themes", "ThemeName", DEFAULT_THEME.ToString());
            RunGCWhilePlaying = Config.Bind("General", "Deactivate Garbage Collector While Playing.", false, "Deactivate the garbage collector during gameplay to prevent lag spikes.");
            ChangePitch = Config.Bind("General", "Change Pitch", false, "Change the pitch on speed changes.");
            OffsetAtDefaultSpeed = Config.Bind("General", "Offset At Default Speed", 0f, "Add an offset when playing charts at 1.0x to compensate for the pitch shift filter latency. RECOMMENDED: 15ms");
            CompatibilityHasConvertedOffsetCheck = Config.Bind("General", nameof(CompatibilityHasConvertedOffsetCheck), false, "DO NOT TOUCH THIS SETTING :) thanks!");
            Config.SettingChanged += ThemeManager.Config_SettingChanged;

            string targetThemePath = Path.Combine(Paths.BepInExRootPath, "Themes");
            string sourceThemePath = Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Themes");
            FileHelper.TryMigrateFolder(sourceThemePath, targetThemePath);

            if (!Directory.Exists(FileHelper.FILE_PATH_TOOTTALLY_APPDATA))
            {
                Plugin.LogInfo($"Couldn't find {FileHelper.FILE_PATH_TOOTTALLY_APPDATA}, creating directory.");
                Directory.CreateDirectory(FileHelper.FILE_PATH_TOOTTALLY_APPDATA);
            }

            if (!CompatibilityHasConvertedOffsetCheck.Value)
            {
                OffsetAtDefaultSpeed.Value = -OffsetAtDefaultSpeed.Value;
                CompatibilityHasConvertedOffsetCheck.Value = true;
            }

            _harmony = new Harmony(Info.Metadata.GUID);
            GameInitializationEvent.Register(Info, TryInitialize);
        }


        private void Update()
        {
            reloadManager.Update();

            if (!reloadManager.IsCurrentlyReloading && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                TootTallyNotifManager.DisplayNotif("Reloading tracks...");
                reloadManager.ReloadAll(new ProgressCallbacks
                {
                    onComplete = () =>
                    {
                        TootTallyNotifManager.DisplayNotif("Reloading complete!");
                    },
                    onError = err =>
                    {
                        TootTallyNotifManager.DisplayNotif($"Reloading failed! {err.Message}");
                    }
                });
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
            {
                TootTallyNotifManager.DisplayNotif("Forcing garbage collection.");
                for (int i = 0; i < 9; i++)
                    GC.Collect(i % 3, GCCollectionMode.Forced);
            }
        }

        /// <summary>
        /// Obsolete; call <see cref="M:TootTallyCore.Utils.Helpers.ReloadManager.ReloadAll(TootTallyCore.Utils.Helpers.IProgressCallbacks)"/>
        /// instead
        /// </summary>
        [Obsolete("Use ReloadManager#ReloadAll instead")]
        public void ReloadTracks()
        {
            reloadManager.ReloadAll(null);
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

            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {Application.version}");
        }
    }
}
