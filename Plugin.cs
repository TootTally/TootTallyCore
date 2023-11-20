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
using TootTallyCore.Utils.TootTallyModules;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;

namespace TootTallyCore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("TootTally")]
    public class Plugin : BaseUnityPlugin
    {
        public static int BUILDDATE = 20231119;

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

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            ShouldShowNotifs = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            DebugMode = Config.Bind("General", "Debug Mode", false, "Add extra logging information for debugging.");

            _harmony = new Harmony(Info.Metadata.GUID);
            GameInitializationEvent.Register(Info, TryInitialize);
        }

        public void ReloadTracks() => TrackLookup.reload();

        private void TryInitialize()
        {
            AssetManager.LoadAssets(Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets"));
            //AssetBundleManager.LoadAssets(Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets/TootTallyAssets"));
            _harmony.PatchAll(typeof(GameObjectFactory));
            gameObject.AddComponent<TootTallyNotifManager>();
            gameObject.AddComponent<TootTallyAnimationManager>();

            TootTallyModuleManager.LoadModules();
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {Application.version}");
        }


    }
}
