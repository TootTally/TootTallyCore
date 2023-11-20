using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Runtime.Remoting.Messaging;
using TootTallyCore.APIServices;
using TootTallyCore.Utils.TootTallyModules;
using UnityEngine;
using UnityEngine.PostProcessing;
using static UnityEngine.Analytics.Analytics;

namespace TootTallyCore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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

            _harmony = new Harmony(Info.Metadata.GUID);
            GameInitializationEvent.Register(Info, TryInitialize);
        }

        public void ReloadTracks() => TrackLookup.reload();

        private void TryInitialize()
        {
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {Application.version}");
        }
    }
}
