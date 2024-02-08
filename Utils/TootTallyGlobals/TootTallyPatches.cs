using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Video;

namespace TootTallyCore.Utils.TootTallyGlobals
{
    public static class TootTallyPatches
    {
        [HarmonyPatch(typeof(GameController), nameof(GameController.buildNotes))]
        [HarmonyPrefix]
        public static void FixAudioLatency(GameController __instance)
        {
            if (GlobalVariables.practicemode == 1 && !GlobalVariables.turbomode)
                __instance.latency_offset = GlobalVariables.localsettings.latencyadjust * 0.001f * TootTallyGlobalVariables.gameSpeedMultiplier;
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.doFastScreenShake))]
        [HarmonyPrefix]
        private static bool RemoveTheGodDamnShake() => false;

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPrefix]
        public static void GameControllerPrefixPatch(GameController __instance)
        {
            if (GlobalVariables.turbomode)
                TootTallyGlobalVariables.gameSpeedMultiplier = 2f;
            else if (GlobalVariables.practicemode != 1f)
                TootTallyGlobalVariables.gameSpeedMultiplier = GlobalVariables.practicemode;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            if (!__instance.freeplay) return;

            TootTallyGlobalVariables.gameSpeedMultiplier = __instance.smooth_scrolling_move_mult = 1f;
        }

        [HarmonyPatch(typeof(BGController), nameof(BGController.setUpBGControllerRefsDelayed))]
        [HarmonyPostfix]
        public static void OnSetUpBGControllerRefsDelayedPostFix(BGController __instance)
        {
            VideoPlayer videoPlayer = null;
            try
            {
                GameObject bgObj = GameObject.Find("BGCameraObj").gameObject;
                videoPlayer = bgObj.GetComponentInChildren<VideoPlayer>();
            }
            catch (Exception e)
            {
                Plugin.LogWarning(e.ToString());
                Plugin.LogWarning("Couldn't find VideoPlayer in background");
            }

            if (videoPlayer != null)
                videoPlayer.playbackSpeed = TootTallyGlobalVariables.gameSpeedMultiplier;

            //Have to set the speed here because the pitch is changed in 2 different places? one time during GC.Start and one during GC.loadAssetBundleResources... Derp
            if (TootTallyGlobalVariables.gameSpeedMultiplier != 1)
            {
                __instance.gamecontroller.smooth_scrolling_move_mult = TootTallyGlobalVariables.gameSpeedMultiplier;
                __instance.gamecontroller.musictrack.pitch = TootTallyGlobalVariables.gameSpeedMultiplier; // SPEEEEEEEEEEEED
                __instance.gamecontroller.breathmultiplier = TootTallyGlobalVariables.gameSpeedMultiplier;
                Plugin.LogInfo("GameSpeed set to " + TootTallyGlobalVariables.gameSpeedMultiplier);
            }

        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.fixAudioMixerStuff))]
        [HarmonyPrefix]
        public static bool OnFixAudioMixerStuffPostFix(GameController __instance)
        {
            if (!Plugin.Instance.ChangePitch.Value)
            {
                __instance.musictrack.outputAudioMixerGroup = __instance.audmix_bgmus_pitchshifted;
                __instance.musictrack.volume = GlobalVariables.localsettings.maxvolume_music;
                __instance.audmix.SetFloat("pitchShifterMult", 1f / TootTallyGlobalVariables.gameSpeedMultiplier);
                __instance.audmix.SetFloat("mastervol", Mathf.Log10(GlobalVariables.localsettings.maxvolume) * 40f);
                __instance.audmix.SetFloat("trombvol", Mathf.Log10(GlobalVariables.localsettings.maxvolume_tromb) * 60f + 12f);
                __instance.audmix.SetFloat("airhornvol", (GlobalVariables.localsettings.maxvolume_airhorn - 1f) * 80f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.startDance))]
        [HarmonyPostfix]
        public static void OnGameControllerStartDanceFixSpeedBackup(GameController __instance)
        {
            if (__instance.musictrack.pitch != TootTallyGlobalVariables.gameSpeedMultiplier)
            {
                __instance.smooth_scrolling_move_mult = TootTallyGlobalVariables.gameSpeedMultiplier;
                __instance.musictrack.pitch = TootTallyGlobalVariables.gameSpeedMultiplier;
                __instance.breathmultiplier = TootTallyGlobalVariables.gameSpeedMultiplier;
                Plugin.LogInfo("BACKUP: GameSpeed set to " + TootTallyGlobalVariables.gameSpeedMultiplier);
            }
        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        private static void DisableGarbageCollector()
        {
            if (Plugin.Instance.RunGCWhilePlaying.Value) return;

            if (IsGCEnabled)
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;

        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        private static void EnableGarbageCollectorOnPause()
        {
            if (Plugin.Instance.RunGCWhilePlaying.Value) return;

            if (!IsGCEnabled)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.resumeTrack))]
        [HarmonyPostfix]
        private static void DisableGarbageCollectorOnResumeTrack()
        {
            if (Plugin.Instance.RunGCWhilePlaying.Value) return;

            if (IsGCEnabled)
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        private static void EnableGarbageCollectorOnLevelSelectEnter()
        {
            if (Plugin.Instance.RunGCWhilePlaying.Value) return;

            if (!IsGCEnabled)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

        }

        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
        [HarmonyPostfix]
        private static void EnableGarbageCollectorOnMultiplayerEnter()
        {
            if (Plugin.Instance.RunGCWhilePlaying.Value) return;

            if (!IsGCEnabled)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        private static bool IsGCEnabled => GarbageCollector.GCMode == GarbageCollector.Mode.Enabled;
    }
}
