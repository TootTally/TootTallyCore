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
            {
                float fixedLatency;
                if (TootTallyGlobalVariables.gameSpeedMultiplier == 1)
                    fixedLatency = (GlobalVariables.localsettings.latencyadjust + Plugin.Instance.OffsetAtDefaultSpeed.Value) * 0.001f;
                else
                    fixedLatency = GlobalVariables.localsettings.latencyadjust * 0.001f * TootTallyGlobalVariables.gameSpeedMultiplier;
                Plugin.LogInfo($"Fixed audio latency from: {__instance.latency_offset} to {fixedLatency}");
                __instance.latency_offset = fixedLatency;
            }
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
            else
                // Fix edge case where this isn't being set
                // appropriately when not using the leaderboard mod.
                TootTallyGlobalVariables.gameSpeedMultiplier = 1f;
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
                OnGameControllerReadySetGameSpeed(__instance.gamecontroller, TootTallyGlobalVariables.gameSpeedMultiplier);
                Plugin.LogInfo("GameSpeed set to " + TootTallyGlobalVariables.gameSpeedMultiplier);
                SetGCTotalTrackTimeString(__instance.gamecontroller);
            }

        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.fixAudioMixerStuff))]
        [HarmonyPrefix]
        public static bool OnFixAudioMixerStuffPostFix(GameController __instance)
        {
            if (!Plugin.Instance.ChangePitch.Value)
            {
                Plugin.LogInfo($"Changed audmix to pitchshifted mixer");
                __instance.musictrack.outputAudioMixerGroup = __instance.audmix_bgmus_pitchshifted;
                __instance.musictrack.volume = GlobalVariables.localsettings.maxvolume_music * (TootTallyGlobalVariables.gameSpeedMultiplier == 1 ? .75f : 1f); //For some reasons 1x speed is louder
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
                OnGameControllerReadySetGameSpeed(__instance, TootTallyGlobalVariables.gameSpeedMultiplier);
                Plugin.LogInfo("BACKUP: GameSpeed set to " + TootTallyGlobalVariables.gameSpeedMultiplier);
                SetGCTotalTrackTimeString(__instance);
            }
        }



        private static double _songTime;
        private static float _songLength;
        private static bool _startSongTime;

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPostfix]
        public static void OnGameControllerUpdateFixPitchAndBreathing(GameController __instance)
        {
            if (_startSongTime && !__instance.paused && !__instance.quitting && !__instance.retrying && !__instance.freeplay && !TootTallyGlobalVariables.isTournamentHosting)
            {
                _songTime += Time.deltaTime * TootTallyGlobalVariables.gameSpeedMultiplier;
                if (!__instance.level_finished && !__instance.musictrack.isPlaying && _songTime >= _songLength)
                {
                    Plugin.LogInfo("Toottally forced level_finished to prevent softlock.");
                    __instance.levelendtime = 0.001f;
                    __instance.musictrack.time = 0.01f;
                    __instance.level_finished = true;
                    __instance.curtainc.closeCurtain(true);
                    if (__instance.totalscore < 0)
                        __instance.totalscore = 0;
                    GlobalVariables.gameplay_scoretotal = __instance.totalscore;
                    if (GlobalVariables.localsettings.acc_autotoot)
                        __instance.maxlevelscore = Mathf.FloorToInt(__instance.maxlevelscore * 0.5f);
                    GlobalVariables.gameplay_scoreperc = __instance.totalscore / __instance.maxlevelscore;
                    GlobalVariables.gameplay_notescores = new int[]
                    {
                        __instance.scores_F,
                        __instance.scores_D,
                        __instance.scores_C,
                        __instance.scores_B,
                        __instance.scores_A
                    };
                }
            }


            float value = 0;
            if (!__instance.noteplaying && __instance.breathcounter >= 0f)
            {
                if (!__instance.outofbreath)
                    value = Time.deltaTime * (1 - TootTallyGlobalVariables.gameSpeedMultiplier) * 8.5f;
                else
                    value = Time.deltaTime * (1 - TootTallyGlobalVariables.gameSpeedMultiplier) * .29f;
            }
            __instance.breathcounter += value;

            if (__instance.breathcounter >= 1f) { __instance.breathcounter = .99f; }
            if (__instance.outofbreath && __instance.breathcounter < 0f) { __instance.breathcounter = .01f; }

            if (__instance.noteplaying && Plugin.Instance.ChangePitch.Value)
                __instance.currentnotesound.pitch *= TootTallyGlobalVariables.gameSpeedMultiplier;
        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        private static void OnGameControllerStartSetupTimer(GameController __instance)
        {
            _startSongTime = false;
            if (__instance == null || __instance.freeplay || __instance.musictrack == null || __instance.musictrack.clip == null) return;
            _songLength = __instance.musictrack.clip.length;
            _songTime = 0;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.playsong))]
        [HarmonyPostfix]
        public static void OnGameControllerStartSongStartSongTime(GameController __instance)
        {
            if (__instance.freeplay || TootTallyGlobalVariables.isSpectating || TootTallyGlobalVariables.isReplaying) return;

            Plugin.LogInfo($"Starting song time.");
            _songLength = __instance.musictrack.clip.length;
            _startSongTime = true;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.updateTimeCounter))]
        [HarmonyPostfix]
        public static void OnUpdateTimeCounterSetTimeElapsedString(float elapsedtime, GameController __instance)
        {
            if (__instance.freeplay) return;
            elapsedtime /= TootTallyGlobalVariables.gameSpeedMultiplier;
            float num = Mathf.Floor(elapsedtime * 0.016666668f);
            int num2 = (int)elapsedtime - (int)num * 60;
            string text = num2.ToString();
            if (num2 < 10)
                text = "0" + text;

            __instance.timeelapsed_shad.text = $"{num}:{text} / {__instance.totaltracktimestring}";
            __instance.timeelapsed.text = $"{num}:{text} <color=#444>/</color> {__instance.totaltracktimestring}";
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void OnGameControllerStartSetTitleWithSpeed(GameController __instance)
        {
            if (TootTallyGlobalVariables.gameSpeedMultiplier == 1 || __instance.freeplay) return;
            var text = $" [{TootTallyGlobalVariables.gameSpeedMultiplier:0.00}x]";
            __instance.songtitle.text = __instance.songtitleshadow.text += text;
        }

        private static void OnGameControllerReadySetGameSpeed(GameController __instance, float speed)
        {
            __instance.smooth_scrolling_move_mult = speed;
            __instance.musictrack.pitch = speed;
            __instance.breathmultiplier = speed;
        }

        public static void SetGCTotalTrackTimeString(GameController __instance)
        {
            if (__instance.freeplay) return;
            float num = __instance.levelendtime / TootTallyGlobalVariables.gameSpeedMultiplier;
            float num2 = Mathf.Floor(num / 60f);
            int num3 = (int)num - (int)num2 * 60;
            string text = num3.ToString();
            if (num3 < 10)
                text = "0" + text;
            __instance.totaltracktimestring = $"{num2}:{text}";
        }

        #region GC Stuff

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        private static void DisableGarbageCollector(GameController __instance)
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
        #endregion
    }
}
