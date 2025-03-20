using BepInEx.Configuration;
using HarmonyLib;
using System;
using TootTallyCore.Utils.Assets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallyCore.Graphics;

namespace TootTallyCore
{
    public static class ThemeManager
    {
        public static Action OnThemeRefreshEvents;

        private const string CONFIG_FIELD = "Themes";
        private const string DEFAULT_THEME = "Default";
        public static Text songyear, songgenre, songcomposer, songtempo, songduration, songdesctext;
        private static string _currentTheme;
        private static bool _isInitialized;

        public static void SetTheme(string themeName)
        {
            _currentTheme = themeName;
            Theme.isDefault = false;
            switch (themeName)
            {
                case "Day":
                    Theme.SetDayTheme();
                    break;
                case "Night":
                    Theme.SetNightTheme();
                    break;
                case "Random":
                    Theme.SetRandomTheme();
                    break;
                case "Default":
                    Theme.SetDefaultTheme();
                    break;
                default:
                    Theme.SetCustomTheme(themeName);
                    break;
            }
            GameObjectFactory.UpdatePrefabTheme();
        }


        public static void Config_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (_currentTheme == Plugin.Instance.ThemeName.Value) return; //skip if theme did not change

            _currentTheme = Plugin.Instance.ThemeName.Value;
            RefreshTheme();
        }

        public static void RefreshTheme()
        {
            SetTheme(_currentTheme);
            OnThemeRefreshEvents?.Invoke();
            TootTallyNotifManager.DisplayNotif("Theme Reloaded!");
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPrefix]
        public static void Initialize()
        {
            if (_isInitialized) return;

            SetTheme(Plugin.Instance.ThemeName.Value);
            _isInitialized = true;
        }

        private static AudioSource _btnClickSfx;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void ChangeThemeOnLevelSelectControllerStartPostFix(LevelSelectController __instance)
        {
            if (Theme.isDefault) return;

            _btnClickSfx = __instance.hoversfx;

            #region ScoreText
            try
            {
                foreach (GameObject btn in __instance.btns)
                    btn.transform.Find("ScoreText").gameObject.GetComponent<Text>().color = Theme.colors.leaderboard.text;
            } catch (Exception e)
            {
                Plugin.LogError("ScoreText theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region SongButton
            try
            {
                GameObject btnBGPrefab = UnityEngine.Object.Instantiate(__instance.btnbgs[0].gameObject);
                //UnityEngine.Object.DestroyImmediate(btnBGPrefab.transform.Find("Image").gameObject);

                for (int i = 0; i < 7; i++) //songbuttons only, not the arrow ones
                {
                    Image img = __instance.btnbgs[i];
                    img.sprite = AssetManager.GetSprite("SongButtonBackground.png");
                    img.transform.parent.GetChild(2).GetComponent<Text>().color = i == 0 ? Theme.colors.songButton.textOver : Theme.colors.songButton.text;

                    GameObject btnBGShadow = UnityEngine.Object.Instantiate(btnBGPrefab, img.gameObject.transform.parent);
                    btnBGShadow.name = "Shadow";
                    OverwriteGameObjectSpriteAndColor(btnBGShadow, "SongButtonShadow.png", Theme.colors.songButton.shadow);

                    GameObject btnBGOutline = UnityEngine.Object.Instantiate(btnBGPrefab, img.gameObject.transform);
                    btnBGOutline.name = "Outline";
                    OverwriteGameObjectSpriteAndColor(btnBGOutline, "SongButtonOutline.png", i == 0 ? Theme.colors.songButton.outlineOver : Theme.colors.songButton.outline);

                    img.transform.Find("score-zone").GetComponent<Image>().color = Theme.colors.songButton.square;
                    img.color = Theme.colors.songButton.background;
                }

                for (int i = 7; i < __instance.btnbgs.Length; i++) //these are the arrow ones :}
                    __instance.btnbgs[i].color = Theme.colors.songButton.background;
                UnityEngine.Object.DestroyImmediate(btnBGPrefab);
            }
            catch (Exception e)
            {
                Plugin.LogError("SongButton theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region SongTitle
            try
            {
                __instance.songtitlebar.GetComponent<Image>().color = Theme.colors.title.titleBar;
                __instance.scenetitle.GetComponent<Text>().color = Theme.colors.title.titleShadow;
                __instance.longsongtitle.color = Theme.colors.title.songName;
                __instance.longsongtitle_dropshadow.color = Theme.colors.title.titleShadow;
                __instance.longsongtitle.GetComponent<Outline>().effectColor = Theme.colors.title.outline;
            }
            catch (Exception e)
            {
                Plugin.LogError("Song Title theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region Lines
            try
            {
                GameObject lines = __instance.btnspanel.transform.Find("RightLines").gameObject;
                lines.GetComponent<RectTransform>().anchoredPosition += new Vector2(-2, 0);
                LineRenderer redLine = lines.transform.Find("Red").GetComponent<LineRenderer>();
                redLine.startColor = Theme.colors.leaderboard.panelBody;
                redLine.endColor = Theme.colors.leaderboard.scoresBody;
                for (int i = 1; i < 8; i++)
                {
                    LineRenderer yellowLine = lines.transform.Find("Yellow" + i).GetComponent<LineRenderer>();
                    yellowLine.startColor = Theme.colors.leaderboard.panelBody;
                    yellowLine.endColor = Theme.colors.leaderboard.scoresBody;
                }
            }
            catch (Exception e)
            {
                Plugin.LogError("Lines theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region Capsules
            try
            {
                GameObject capsules = GameObject.Find("MainCanvas/FullScreenPanel/capsules").gameObject;
                GameObject capsulesPrefab = UnityEngine.Object.Instantiate(capsules);

                foreach (Transform t in capsulesPrefab.transform) UnityEngine.Object.Destroy(t.gameObject);
                RectTransform rectTrans = capsulesPrefab.GetComponent<RectTransform>();
                rectTrans.localScale = Vector3.one;
                rectTrans.anchoredPosition = Vector2.zero;


                GameObject capsulesYearShadow = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesYearShadow, "YearCapsule.png", Theme.colors.capsules.yearShadow);
                capsulesYearShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

                GameObject capsulesYear = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesYear, "YearCapsule.png", Theme.colors.capsules.year);

                songyear = UnityEngine.Object.Instantiate(__instance.songyear, capsulesYear.transform);

                GameObject capsulesGenreShadow = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesGenreShadow, "GenreCapsule.png", Theme.colors.capsules.genreShadow);
                capsulesGenreShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

                GameObject capsulesGenre = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesGenre, "GenreCapsule.png", Theme.colors.capsules.genre);
                songgenre = UnityEngine.Object.Instantiate(__instance.songgenre, capsulesGenre.transform);

                GameObject capsulesComposerShadow = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesComposerShadow, "ComposerCapsule.png", Theme.colors.capsules.composerShadow);
                capsulesComposerShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

                GameObject capsulesComposer = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesComposer, "ComposerCapsule.png", Theme.colors.capsules.composer);
                songcomposer = UnityEngine.Object.Instantiate(__instance.songcomposer, capsulesComposer.transform);

                GameObject capsulesTempo = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesTempo, "BPMTimeCapsule.png", Theme.colors.capsules.tempo);
                songtempo = UnityEngine.Object.Instantiate(__instance.songtempo, capsulesTempo.transform);
                songduration = UnityEngine.Object.Instantiate(__instance.songduration, capsulesTempo.transform);

                GameObject capsulesDescTextShadow = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesDescTextShadow, "DescCapsule.png", Theme.colors.capsules.descriptionShadow);
                capsulesDescTextShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

                GameObject capsulesDescText = UnityEngine.Object.Instantiate(capsulesPrefab, capsules.transform);
                OverwriteGameObjectSpriteAndColor(capsulesDescText, "DescCapsule.png", Theme.colors.capsules.description);
                songdesctext = UnityEngine.Object.Instantiate(__instance.songdesctext, capsulesDescText.transform);

                UnityEngine.Object.DestroyImmediate(capsules.GetComponent<Image>());
                UnityEngine.Object.DestroyImmediate(capsulesPrefab);
            }
            catch (Exception e)
            {
                Plugin.LogError("Capsules theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region PlayButton
            try
            {
                GameObject playButtonBG = __instance.playbtnobj.transform.Find("play-bg").gameObject;
                GameObject playBGPrefab = UnityEngine.Object.Instantiate(playButtonBG, __instance.playbtn.transform);
                foreach (Transform t in playBGPrefab.transform) t.gameObject.SetActive(false);

                GameObject playBackgroundImg = UnityEngine.Object.Instantiate(playBGPrefab, __instance.playbtn.transform);
                playBackgroundImg.name = "playBackground";
                OverwriteGameObjectSpriteAndColor(playBackgroundImg, "PlayBackground.png", Theme.colors.playButton.background);

                GameObject playOutline = UnityEngine.Object.Instantiate(playBGPrefab, __instance.playbtn.transform);
                playOutline.name = "playOutline";
                OverwriteGameObjectSpriteAndColor(playOutline, "PlayOutline.png", Theme.colors.playButton.outline);

                var playText = UnityEngine.Object.Instantiate(__instance.playbtn.transform.GetChild(3), __instance.playbtn.transform).GetChild(0).GetComponent<Text>();
                playText.color = Theme.colors.playButton.text;

                GameObject playShadow = UnityEngine.Object.Instantiate(playBGPrefab, __instance.playbtn.transform);
                playShadow.name = "playShadow";
                OverwriteGameObjectSpriteAndColor(playShadow, "PlayShadow.png", Theme.colors.playButton.shadow);

                playButtonBG.SetActive(false);
                playBGPrefab.SetActive(false);
            }
            catch (Exception e)
            {
                Plugin.LogError("Play button theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region BackButton
            try
            {
                GameObject backButtonBG = __instance.backbutton.transform.Find("button-full").gameObject;
                backButtonBG.name = "BackButton";
                GameObject backBGPrefab = UnityEngine.Object.Instantiate(backButtonBG.transform.GetChild(0).gameObject, __instance.backbutton.transform);
                foreach (Transform t in backBGPrefab.transform) t.gameObject.SetActive(false);

                GameObject backBackgroundImg = UnityEngine.Object.Instantiate(backBGPrefab, __instance.backbutton.transform);
                backBackgroundImg.name = "backBackground";
                OverwriteGameObjectSpriteAndColor(backBackgroundImg, "BackBackground.png", Theme.colors.backButton.background);

                GameObject backOutline = UnityEngine.Object.Instantiate(backBGPrefab, __instance.backbutton.transform);
                backOutline.name = "backOutline";
                OverwriteGameObjectSpriteAndColor(backOutline, "BackOutline.png", Theme.colors.backButton.outline);

                var backText = UnityEngine.Object.Instantiate(__instance.backbutton.transform.GetChild(0).GetChild(2), __instance.backbutton.transform).GetComponent<Text>();
                backText.name = "backText";
                backText.color = Theme.colors.backButton.text;

                GameObject backShadow = UnityEngine.Object.Instantiate(backBGPrefab, __instance.backbutton.transform);
                backShadow.name = "backShadow";
                OverwriteGameObjectSpriteAndColor(backShadow, "BackShadow.png", Theme.colors.backButton.shadow);

                backButtonBG.SetActive(false);
                backBGPrefab.SetActive(false);
            }
            catch (Exception e)
            {
                Plugin.LogError("Back button theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region RandomButton
            try
            {
                __instance.btnrandom.transform.Find("Text").GetComponent<Text>().color = Theme.colors.randomButton.text;
                __instance.btnrandom.transform.Find("icon").GetComponent<Image>().color = Theme.colors.randomButton.text;
                __instance.btnrandom.transform.Find("btn-shadow").GetComponent<Image>().color = Theme.colors.randomButton.shadow;

                GameObject randomButtonPrefab = UnityEngine.Object.Instantiate(__instance.btnrandom.transform.Find("btn").gameObject);
                RectTransform randomRectTransform = randomButtonPrefab.GetComponent<RectTransform>();
                randomRectTransform.anchoredPosition = Vector2.zero;
                randomRectTransform.localScale = Vector3.one;
                __instance.btnrandom.transform.Find("btn").gameObject.SetActive(false);

                GameObject randomButtonBackground = UnityEngine.Object.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
                randomButtonBackground.name = "RandomBackground";
                OverwriteGameObjectSpriteAndColor(randomButtonBackground, "RandomBackground.png", Theme.colors.randomButton.background);
                __instance.btnrandom.transform.Find("Text").SetParent(randomButtonBackground.transform);
                __instance.btnrandom.transform.Find("icon").SetParent(randomButtonBackground.transform);

                GameObject randomButtonOutline = UnityEngine.Object.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
                randomButtonOutline.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1);
                randomButtonOutline.name = "RandomOutline";
                OverwriteGameObjectSpriteAndColor(randomButtonOutline, "RandomOutline.png", Theme.colors.randomButton.outline);

                /*GameObject randomButtonIcon = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
                randomButtonIcon.name = "RandomIcon";
                OverwriteGameObjectSpriteAndColor(randomButtonIcon, "RandomIcon.png", GameTheme.themeColors.randomButton.text);*/

                UnityEngine.Object.DestroyImmediate(__instance.btnrandom.GetComponent<Image>());
                randomButtonPrefab.SetActive(false);

                EventTrigger randomBtnEvents = __instance.btnrandom.AddComponent<EventTrigger>();
                EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
                pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
                pointerEnterEvent.callback.AddListener((data) => OnPointerEnterRandomEvent(__instance));
                randomBtnEvents.triggers.Add(pointerEnterEvent);

                EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
                pointerExitEvent.eventID = EventTriggerType.PointerExit;
                pointerExitEvent.callback.AddListener((data) => OnPointerLeaveRandomEvent(__instance));
                randomBtnEvents.triggers.Add(pointerExitEvent);
            }
            catch (Exception e)
            {
                Plugin.LogError("Random button theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region PointerArrow
            try
            {
                GameObject arrowPointerPrefab = UnityEngine.Object.Instantiate(__instance.pointerarrow.gameObject);
                OverwriteGameObjectSpriteAndColor(__instance.pointerarrow.gameObject, "pointerBG.png", Theme.colors.pointer.background);

                GameObject arrowPointerShadow = UnityEngine.Object.Instantiate(arrowPointerPrefab, __instance.pointerarrow.transform);
                arrowPointerShadow.name = "Shadow";
                arrowPointerShadow.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                OverwriteGameObjectSpriteAndColor(arrowPointerShadow, "pointerShadow.png", Theme.colors.pointer.shadow);

                GameObject arrowPointerPointerOutline = UnityEngine.Object.Instantiate(arrowPointerPrefab, __instance.pointerarrow.transform);
                arrowPointerPointerOutline.name = "Outline";
                arrowPointerPointerOutline.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                OverwriteGameObjectSpriteAndColor(arrowPointerPointerOutline, "pointerOutline.png", Theme.colors.pointer.outline);

                UnityEngine.Object.DestroyImmediate(arrowPointerPrefab);
            }
            catch (Exception e)
            {
                Plugin.LogError("Pointer arrow theme couldn't be applied:" + e.Message);
            }
            #endregion

            #region Background
            try
            {
                __instance.bgdots.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 165.5f);
                __instance.bgdots.transform.Find("Image").GetComponent<Image>().color = Theme.colors.background.dots;
                __instance.bgdots.transform.Find("Image (1)").GetComponent<Image>().color = Theme.colors.background.dots;
                __instance.bgdots2.transform.Find("Image").GetComponent<Image>().color = Theme.colors.background.dots2;
                GameObject extraDotsBecauseGameDidntLeanTweenFarEnoughSoWeCanSeeTheEndOfTheTextureFix = UnityEngine.Object.Instantiate(__instance.bgdots.transform.Find("Image").gameObject, __instance.bgdots.transform.Find("Image").transform);
                extraDotsBecauseGameDidntLeanTweenFarEnoughSoWeCanSeeTheEndOfTheTextureFix.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1010);
                GameObject.Find("bgcamera").GetComponent<Camera>().backgroundColor = Theme.colors.background.background;
                GameObject.Find("BG Shape").GetComponent<Image>().color = Theme.colors.background.shape;
                GameObject MainCanvas = GameObject.Find("MainCanvas").gameObject;
                MainCanvas.transform.Find("FullScreenPanel/bgdots_diamond").GetComponent<Image>().color = Theme.colors.background.diamond;
            }
            catch (Exception e)
            {
                Plugin.LogError("Background theme couldn't be applied:" + e.Message);
            }
            #endregion

            //CapsulesTextColor
            songyear.color = Theme.colors.leaderboard.text;
            songgenre.color = Theme.colors.leaderboard.text;
            songduration.color = Theme.colors.leaderboard.text;
            songcomposer.color = Theme.colors.leaderboard.text;
            songtempo.color = Theme.colors.leaderboard.text;
            songdesctext.color = Theme.colors.leaderboard.text;
            OnAdvanceSongsPostFix(__instance);

        }

        #region hoverAndUnHoverSongButtons
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBtn))]
        [HarmonyPostfix]
        public static void OnHoverBtnPostfix(LevelSelectController __instance, int btnnum)
        {
            if (Theme.isDefault) return;
            if (btnnum >= 7)
            {
                __instance.btnbgs[btnnum].GetComponent<Image>().color = Theme.colors.songButton.outline;
                return;
            }
            __instance.btnbgs[btnnum].GetComponent<Image>().color = Theme.colors.songButton.background;
            __instance.btnbgs[btnnum].transform.Find("Outline").GetComponent<Image>().color = Theme.colors.songButton.outlineOver;
            __instance.btnbgs[btnnum].transform.parent.GetChild(2).GetComponent<Text>().color = Theme.colors.songButton.textOver;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverBtn))]
        [HarmonyPostfix]
        public static void OnUnHoverBtnPostfix(LevelSelectController __instance, int btnnum)
        {
            if (Theme.isDefault) return;
            if (btnnum >= 7)
            {
                __instance.btnbgs[btnnum].GetComponent<Image>().color = Theme.colors.songButton.background;
                return;
            }
            __instance.btnbgs[btnnum].GetComponent<Image>().color = Theme.colors.songButton.background;
            __instance.btnbgs[btnnum].transform.Find("Outline").GetComponent<Image>().color = Theme.colors.songButton.outline;
            __instance.btnbgs[btnnum].transform.parent.GetChild(2).GetComponent<Text>().color = Theme.colors.songButton.text;
        }
        #endregion

        #region PlayAndBackEvents
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverPlay))]
        [HarmonyPrefix]
        public static bool OnHoverPlayBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (Theme.isDefault) return true;
            _btnClickSfx.Play();
            __instance.playhovering = true;
            __instance.playbtnobj.transform.Find("playBackground").GetComponent<Image>().color = Theme.colors.playButton.backgroundOver;
            __instance.playbtnobj.transform.Find("playOutline").GetComponent<Image>().color = Theme.colors.playButton.outlineOver;
            __instance.playbtnobj.transform.Find("txt-play/txt-play-front").GetComponent<Text>().color = Theme.colors.playButton.textOver;
            __instance.playbtnobj.transform.Find("playShadow").GetComponent<Image>().color = Theme.colors.playButton.shadowOver;
            return false;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverPlay))]
        [HarmonyPrefix]
        public static bool OnUnHoverPlayBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (Theme.isDefault) return true;
            __instance.playhovering = false;
            __instance.playbtnobj.transform.Find("playBackground").GetComponent<Image>().color = Theme.colors.playButton.background;
            __instance.playbtnobj.transform.Find("playOutline").GetComponent<Image>().color = Theme.colors.playButton.outline;
            __instance.playbtnobj.transform.Find("txt-play/txt-play-front").GetComponent<Text>().color = Theme.colors.playButton.text;
            __instance.playbtnobj.transform.Find("playShadow").GetComponent<Image>().color = Theme.colors.playButton.shadow;
            return false;
        }

        [HarmonyPatch(typeof(BackButtonController), nameof(BackButtonController.onHover))]
        [HarmonyPrefix]
        public static bool OnHoverBackBypassIfThemeNotDefault(BackButtonController __instance, bool hovering)
        {
            if (Theme.isDefault || __instance.gameObject.name == "BackButtonNew") return true; //ButtonNew is from the option menu in HomeScreen
            __instance.txt_back = __instance.gameObject.transform.Find("backText").GetComponent<Text>();
            if (hovering)
            {
                _btnClickSfx.Play();
                __instance.gameObject.transform.Find("backBackground").GetComponent<Image>().color = Theme.colors.backButton.backgroundOver;
                __instance.gameObject.transform.Find("backOutline").GetComponent<Image>().color = Theme.colors.backButton.outlineOver;
                __instance.txt_back.color = Theme.colors.backButton.textOver;
                __instance.gameObject.transform.Find("backShadow").GetComponent<Image>().color = Theme.colors.backButton.shadowOver;
            }
            else
            {
                __instance.gameObject.transform.Find("backBackground").GetComponent<Image>().color = Theme.colors.backButton.background;
                __instance.gameObject.transform.Find("backOutline").GetComponent<Image>().color = Theme.colors.backButton.outline;
                __instance.txt_back.color = Theme.colors.backButton.text;
                __instance.gameObject.transform.Find("backShadow").GetComponent<Image>().color = Theme.colors.backButton.shadow;
            }
            return false;
        }

        public static void OnPointerEnterRandomEvent(LevelSelectController __instance)
        {
            __instance.hoversfx.Play();
            __instance.btnrandom.transform.Find("RandomBackground").GetComponent<Image>().color = Theme.colors.randomButton.backgroundOver;
            __instance.btnrandom.transform.Find("RandomOutline").GetComponent<Image>().color = Theme.colors.randomButton.outlineOver;
            __instance.btnrandom.transform.Find("RandomBackground/icon").GetComponent<Image>().color = Theme.colors.randomButton.textOver;
            __instance.btnrandom.transform.Find("RandomBackground/Text").GetComponent<Text>().color = Theme.colors.randomButton.textOver;
            __instance.btnrandom.transform.Find("btn-shadow").GetComponent<Image>().color = Theme.colors.randomButton.shadowOver;
        }
        public static void OnPointerLeaveRandomEvent(LevelSelectController __instance)
        {
            __instance.btnrandom.transform.Find("RandomBackground").GetComponent<Image>().color = Theme.colors.randomButton.background;
            __instance.btnrandom.transform.Find("RandomOutline").GetComponent<Image>().color = Theme.colors.randomButton.outline;
            __instance.btnrandom.transform.Find("RandomBackground/icon").GetComponent<Image>().color = Theme.colors.randomButton.text;
            __instance.btnrandom.transform.Find("RandomBackground/Text").GetComponent<Text>().color = Theme.colors.randomButton.text;
            __instance.btnrandom.transform.Find("btn-shadow").GetComponent<Image>().color = Theme.colors.randomButton.shadow;
        }

        #endregion

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        public static void OnAdvanceSongsPostFix(LevelSelectController __instance)
        {
            for (int i = 0; i < (Theme.isDefault ? __instance.alltrackslist[__instance.songindex].difficulty : 10); i++)
            {
                if (!Theme.isDefault)
                    __instance.diffstars[i].color = Color.Lerp(Theme.colors.diffStar.gradientStart, Theme.colors.diffStar.gradientEnd, i / 9f);
                else
                    __instance.diffstars[i].color = Color.white;
            }
            if (Theme.isDefault || songyear == null) return;
            songyear.text = __instance.songyear.text;
            songgenre.text = __instance.songgenre.text;
            songduration.text = __instance.songduration.text;
            songcomposer.text = __instance.songcomposer.text;
            songtempo.text = __instance.songtempo.text;
            songdesctext.text = __instance.songdesctext.text;
        }
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
        [HarmonyPostfix]
        public static void OnSortTracksPostFix(LevelSelectController __instance) => OnAdvanceSongsPostFix(__instance);

        [HarmonyPatch(typeof(WaveController), nameof(WaveController.Start))]
        [HarmonyPostfix]
        public static void WaveControllerFuckeryOverwrite(WaveController __instance)
        {
            if (Theme.isDefault) return;

            foreach (SpriteRenderer sr in __instance.wavesprites)
                sr.color = __instance.gameObject.name == "BGWave" ? Theme.colors.background.waves : Theme.colors.background.waves2;
        }

        public static void OverwriteGameObjectSpriteAndColor(GameObject gameObject, string spriteName, Color spriteColor)
        {
            var image = gameObject.GetComponent<Image>();
            image.useSpriteMesh = true;
            image.sprite = AssetManager.GetSprite(spriteName);
            image.color = spriteColor;
        }
    }
}
