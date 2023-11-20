using HarmonyLib;
using Rewired.UI.ControlMapper;
using System;
using System.Linq;
using TMPro;
using TootTallyCore.APIServices;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallyCore.Utils.Assets;

namespace TootTallyCore.Graphics
{
    public static class GameObjectFactory
    {
        private static CustomButton _buttonPrefab;
        private static TextMeshProUGUI _multicoloreTextPrefab, _comfortaaTextPrefab;
        private static GameObject _bubblePrefab;
        private static Slider _verticalSliderPrefab, _sliderPrefab;
        private static Slider _settingsPanelVolumeSlider;
        private static TootTallyNotif _tootTallyNotifPrefab;

        private static GameObject _settingsGraphics, _creditPanel;
        private static TMP_InputField _inputFieldPrefab;

        private static GameObject _overlayPanelPrefab, _userCardPrefab;

        private static bool _isHomeControllerInitialized;
        private static bool _isLevelSelectControllerInitialized;

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphicsHomeController(HomeController __instance)
        {
            _settingsGraphics = __instance.fullsettingspanel.transform.Find("Settings").gameObject;
            _settingsPanelVolumeSlider = GameObject.Instantiate(__instance.set_sld_volume_tromb);
            _settingsPanelVolumeSlider.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(_settingsPanelVolumeSlider);
            _creditPanel = __instance.ext_credits_go.transform.parent.gameObject;
            OnHomeControllerInitialize(__instance);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkGraphicsLevelSelectController(LevelSelectController __instance)
        {
            OnLevelSelectControllerInitialize(__instance);
        }

        public static void OnHomeControllerInitialize(HomeController homeController)
        {

            if (_isHomeControllerInitialized) return;

            SetMulticoloreTextPrefab();
            SetComfortaaTextPrefab();
            SetInputFieldPrefab();
            SetNotificationPrefab();
            SetBubblePrefab();
            SetCustomButtonPrefab();
            SetOverlayPanelPrefab();
            _isHomeControllerInitialized = true;
        }

        public static void OnLevelSelectControllerInitialize(LevelSelectController levelSelectController)
        {
            if (_isLevelSelectControllerInitialized) return;

            Plugin.LogDebug("Generating Slider prefab...");
            SetSliderPrefab();
            Plugin.LogDebug("Generating VerticalSlider prefab...");
            SetVerticalSliderPrefab();
            _isLevelSelectControllerInitialized = true;
        }

        #region SetPrefabs

        private static void SetMulticoloreTextPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject headerCreditText = mainCanvas.transform.Find("FullCreditsPanel/header-credits/Text").gameObject;

            GameObject textHolder = GameObject.Instantiate(headerCreditText);
            textHolder.name = "defaultTextPrefab";
            textHolder.SetActive(true);
            GameObject.DestroyImmediate(textHolder.GetComponent<Text>());
            _multicoloreTextPrefab = textHolder.AddComponent<TextMeshProUGUI>();
            _multicoloreTextPrefab.fontSize = 22;
            _multicoloreTextPrefab.text = "defaultText";
            _multicoloreTextPrefab.font = TMP_FontAsset.CreateFontAsset(headerCreditText.GetComponent<Text>().font);

            _multicoloreTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, .25f);
            _multicoloreTextPrefab.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
            _multicoloreTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, .25f);
            _multicoloreTextPrefab.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            _multicoloreTextPrefab.alignment = TextAlignmentOptions.Center;
            _multicoloreTextPrefab.GetComponent<RectTransform>().sizeDelta = textHolder.GetComponent<RectTransform>().sizeDelta;
            _multicoloreTextPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _multicoloreTextPrefab.richText = true;
            GameObject.DontDestroyOnLoad(_multicoloreTextPrefab);
        }

        private static void SetComfortaaTextPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject advancePanelText = mainCanvas.transform.Find("AdvancedInfoPanel/primary-content/intro/copy").gameObject;

            GameObject textHolder = GameObject.Instantiate(advancePanelText);
            textHolder.name = "ComfortaaTextPrefab";
            textHolder.SetActive(true);
            GameObject.DestroyImmediate(textHolder.GetComponent<Text>());
            _comfortaaTextPrefab = textHolder.AddComponent<TextMeshProUGUI>();
            _comfortaaTextPrefab.fontSize = 22;
            _comfortaaTextPrefab.text = "DefaultText";
            _comfortaaTextPrefab.font = TMP_FontAsset.CreateFontAsset(advancePanelText.GetComponent<Text>().font);

            _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, .25f);
            _comfortaaTextPrefab.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
            _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, .25f);
            _comfortaaTextPrefab.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            _comfortaaTextPrefab.alignment = TextAlignmentOptions.Center;
            _comfortaaTextPrefab.GetComponent<RectTransform>().sizeDelta = textHolder.GetComponent<RectTransform>().sizeDelta;
            _comfortaaTextPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _comfortaaTextPrefab.richText = true;
            _comfortaaTextPrefab.enableWordWrapping = false;
            GameObject.DontDestroyOnLoad(_comfortaaTextPrefab);
        }

        private static void SetInputFieldPrefab()
        {
            var inputHolder = new GameObject("InputFieldHolder");
            var rectHolder = inputHolder.AddComponent<RectTransform>();
            rectHolder.anchoredPosition = Vector2.zero;
            rectHolder.sizeDelta = new Vector2(350, 50);
            var inputImageHolder = GameObject.Instantiate(inputHolder, inputHolder.transform);
            var inputTextHolder = GameObject.Instantiate(inputImageHolder, inputHolder.transform);
            inputImageHolder.name = "Image";
            inputTextHolder.name = "Text";

            _inputFieldPrefab = inputHolder.AddComponent<TMP_InputField>();

            rectHolder.anchorMax = rectHolder.anchorMin = Vector2.zero;

            //pain... @_@
            _inputFieldPrefab.image = inputImageHolder.AddComponent<Image>();
            RectTransform rectImage = inputImageHolder.GetComponent<RectTransform>();

            rectImage.anchorMin = rectImage.anchorMax = rectImage.pivot = Vector2.zero;
            rectImage.anchoredPosition = new Vector2(0, 4);
            rectImage.sizeDelta = new Vector2(350, 2);

            RectTransform rectText = inputTextHolder.GetComponent<RectTransform>();
            rectText.anchoredPosition = rectText.anchorMin = rectText.anchorMax = rectText.pivot = Vector2.zero;
            rectText.sizeDelta = new Vector2(350, 50);

            _inputFieldPrefab.textComponent = GameObjectFactory.CreateSingleText(inputTextHolder.transform, $"TextLabel", "", Color.white);
            _inputFieldPrefab.textComponent.rectTransform.pivot = new Vector2(0, 0.5f);
            _inputFieldPrefab.textComponent.alignment = TextAlignmentOptions.Left;
            _inputFieldPrefab.textComponent.margin = new Vector4(5, 0, 0, 0);
            _inputFieldPrefab.textComponent.enableWordWrapping = true;
            _inputFieldPrefab.textViewport = _inputFieldPrefab.textComponent.rectTransform;

            GameObject.DontDestroyOnLoad(_inputFieldPrefab);
        }

        private static void SetNotificationPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject bufferPanel = mainCanvas.transform.Find("SettingsPanel/buffer_panel/window border").gameObject;

            GameObject gameObjectHolder = GameObject.Instantiate(bufferPanel);
            gameObjectHolder.name = "NotificationPrefab";
            GameObject.DestroyImmediate(gameObjectHolder.transform.Find("Window Body/all_settiings").gameObject);

            _tootTallyNotifPrefab = gameObjectHolder.AddComponent<TootTallyNotif>();
            RectTransform popUpNorifRectTransform = _tootTallyNotifPrefab.GetComponent<RectTransform>();
            popUpNorifRectTransform.anchoredPosition = new Vector2(695, -700);
            popUpNorifRectTransform.sizeDelta = new Vector2(450, 200);

            TMP_Text notifText = GameObject.Instantiate(_multicoloreTextPrefab, _tootTallyNotifPrefab.transform);
            notifText.name = "NotifText";
            notifText.gameObject.GetComponent<RectTransform>().sizeDelta = popUpNorifRectTransform.sizeDelta;
            notifText.gameObject.SetActive(true);

            gameObjectHolder.SetActive(false);

            GameObject.DontDestroyOnLoad(_tootTallyNotifPrefab);
        }

        private static void SetBubblePrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject bufferPanel = mainCanvas.transform.Find("SettingsPanel/buffer_panel/window border").gameObject;

            _bubblePrefab = GameObject.Instantiate(bufferPanel);
            _bubblePrefab.name = "BubblePrefab";
            GameObject.DestroyImmediate(_bubblePrefab.transform.Find("Window Body/all_settiings").gameObject);

            CanvasGroup canvasGroup = _bubblePrefab.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            RectTransform bubblePrefabRect = _bubblePrefab.GetComponent<RectTransform>();
            bubblePrefabRect.sizeDelta = new Vector2(250, 100);
            bubblePrefabRect.pivot = new Vector2(1, 0);



            var text = GameObject.Instantiate(_multicoloreTextPrefab, _bubblePrefab.transform.Find("Window Body"));
            text.name = "BubbleText";
            text.maskable = false;
            text.enableWordWrapping = true;
            text.rectTransform.sizeDelta = bubblePrefabRect.sizeDelta;
            text.gameObject.SetActive(true);

            _bubblePrefab.SetActive(false);

            GameObject.DontDestroyOnLoad(_bubblePrefab);
        }


        private static void SetCustomButtonPrefab()
        {
            GameObject settingBtn = _settingsGraphics.transform.Find("GRAPHICS/btn_opengraphicspanel").gameObject;

            GameObject gameObjectHolder = UnityEngine.Object.Instantiate(settingBtn);

            var tempBtn = gameObjectHolder.GetComponent<Button>();
            var oldBtnColors = tempBtn.colors;

            UnityEngine.Object.DestroyImmediate(tempBtn);

            var myBtn = gameObjectHolder.AddComponent<Button>();
            myBtn.colors = oldBtnColors;


            _buttonPrefab = gameObjectHolder.AddComponent<CustomButton>();
            _buttonPrefab.ConstructNewButton(gameObjectHolder.GetComponent<Button>(), gameObjectHolder.GetComponentInChildren<Text>());

            gameObjectHolder.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(gameObjectHolder);
        }

        private static void SetVerticalSliderPrefab()
        {
            Slider defaultSlider = GameObject.Find("MainCanvas/FullScreenPanel/Slider").GetComponent<Slider>(); //yoink

            _verticalSliderPrefab = GameObject.Instantiate(defaultSlider);
            _verticalSliderPrefab.direction = Slider.Direction.TopToBottom;

            RectTransform sliderRect = _verticalSliderPrefab.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(25, 745);
            sliderRect.anchoredPosition = new Vector2(300, 0);

            RectTransform fillAreaRect = _verticalSliderPrefab.transform.Find("Fill Area").GetComponent<RectTransform>();
            fillAreaRect.sizeDelta = new Vector2(-19, -2);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);

            RectTransform handleSlideAreaRect = _verticalSliderPrefab.transform.Find("Handle Slide Area").GetComponent<RectTransform>();
            RectTransform handleRect = handleSlideAreaRect.gameObject.transform.Find("Handle").GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(40, 40);
            handleRect.pivot = Vector2.zero;
            handleRect.anchorMax = Vector2.zero;
            GameObject handle = GameObject.Instantiate(handleRect.gameObject, _verticalSliderPrefab.transform);
            handle.name = "Handle";
            RectTransform backgroundSliderRect = _verticalSliderPrefab.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            _verticalSliderPrefab.value = 0f;
            _verticalSliderPrefab.minValue = -0.05f;
            _verticalSliderPrefab.maxValue = 1.04f;
            _verticalSliderPrefab.onValueChanged = new Slider.SliderEvent();
            _verticalSliderPrefab.gameObject.SetActive(false);

            DestroyFromParent(_verticalSliderPrefab.gameObject, "Handle Slide Area/Handle");

            GameObject.DontDestroyOnLoad(_verticalSliderPrefab);
        }
        private static void SetSliderPrefab()
        {
            Slider defaultSlider = GameObject.Find("MainCanvas/FullScreenPanel/Slider").GetComponent<Slider>(); //yoink

            _sliderPrefab = GameObject.Instantiate(defaultSlider);
            //_sliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;

            RectTransform sliderRect = _sliderPrefab.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(-200, 0);

            RectTransform backgroundSliderRect = _sliderPrefab.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            _sliderPrefab.value = 1f;
            _sliderPrefab.minValue = 0f;
            _sliderPrefab.maxValue = 2f;
            _sliderPrefab.onValueChanged = new Slider.SliderEvent();
            _sliderPrefab.gameObject.SetActive(false);


            GameObject.DontDestroyOnLoad(_sliderPrefab);
        }

        private static void SetOverlayPanelPrefab()
        {
            _overlayPanelPrefab = GameObject.Instantiate(_creditPanel);
            _overlayPanelPrefab.name = "OverlayPanelPrefab";
            _overlayPanelPrefab.transform.localScale = Vector3.one;
            _overlayPanelPrefab.SetActive(false);
            _overlayPanelPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);

            GameObject fsLatencyPanel = _overlayPanelPrefab.transform.Find("FSLatencyPanel").gameObject;
            fsLatencyPanel.SetActive(true);
            GameObject.DestroyImmediate(fsLatencyPanel.transform.Find("LatencyBG2").gameObject);
            //fsLatencyPanel.transform.Find("LatencyBG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.border;

            GameObject latencyFGPanel = fsLatencyPanel.transform.Find("LatencyFG").gameObject; //this where most objects are located
            //latencyFGPanel.GetComponent<Image>().color = GameTheme.themeColors.notification.background;
            DestroyFromParent(latencyFGPanel, "page2");
            DestroyFromParent(latencyFGPanel, "page3");
            DestroyFromParent(latencyFGPanel, "page4");
            DestroyFromParent(latencyFGPanel, "page5");
            DestroyFromParent(latencyFGPanel, "PREV");

            Text title = latencyFGPanel.transform.Find("title").gameObject.GetComponent<Text>();
            Text subtitle = latencyFGPanel.transform.Find("subtitle").gameObject.GetComponent<Text>();
            title.text = "TootTally Panel";
            //title.color = GameTheme.themeColors.notification.defaultText;
            subtitle.text = "TootTally Panel Early Version Description";
            //subtitle.color = GameTheme.themeColors.notification.defaultText;


            GameObject mainPage = latencyFGPanel.transform.Find("page1").gameObject;
            GameObject.DestroyImmediate(mainPage.GetComponent<HorizontalLayoutGroup>());
            VerticalLayoutGroup vgroup = mainPage.AddComponent<VerticalLayoutGroup>();
            vgroup.childForceExpandHeight = vgroup.childScaleHeight = vgroup.childControlHeight = false;
            vgroup.childForceExpandWidth = vgroup.childScaleWidth = vgroup.childControlWidth = false;
            vgroup.padding.left = (int)(mainPage.GetComponent<RectTransform>().sizeDelta.x / 2) - 125;
            vgroup.spacing = 20;
            mainPage.name = "MainPage";

            DestroyFromParent(mainPage, "col1");
            DestroyFromParent(mainPage, "col2");
            DestroyFromParent(mainPage, "col3");
            DestroyFromParent(latencyFGPanel, "CloseBtn");
            DestroyFromParent(latencyFGPanel, "NEXT");

            GameObject.DontDestroyOnLoad(_overlayPanelPrefab);
        }


        private static void SetUserCardPrefab()
        {
            _userCardPrefab = GameObject.Instantiate(_overlayPanelPrefab.transform.Find("FSLatencyPanel").gameObject);
            _userCardPrefab.name = "UserCardPrefab";
            _userCardPrefab.GetComponent<Image>().color = new Color(0, 0, 0, 0);


            var fgRect = _userCardPrefab.transform.Find("LatencyFG").GetComponent<RectTransform>();
            var bgRect = _userCardPrefab.transform.Find("LatencyBG").GetComponent<RectTransform>();
            fgRect.GetComponent<Image>().maskable = bgRect.GetComponent<Image>().maskable = true;
            var size = new Vector2(360, 100);
            fgRect.sizeDelta = size;
            fgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = size + (Vector2.one * 10f);
            bgRect.anchoredPosition = Vector2.zero;
            _userCardPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            var horizontalContentHolder = fgRect.gameObject;
            DestroyFromParent(horizontalContentHolder, "title");
            DestroyFromParent(horizontalContentHolder, "subtitle");

            var horizontalLayoutGroup = horizontalContentHolder.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
            horizontalLayoutGroup.spacing = 20f;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayoutGroup.childControlHeight = horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childForceExpandHeight = horizontalLayoutGroup.childForceExpandWidth = false;



            var contentHolderLeft = horizontalContentHolder.transform.Find("MainPage").gameObject;
            contentHolderLeft.name = "LeftContent";

            var verticalLayoutGroup = contentHolderLeft.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
            verticalLayoutGroup.spacing = 4f;
            verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            verticalLayoutGroup.childControlHeight = verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childForceExpandHeight = verticalLayoutGroup.childForceExpandWidth = true;

            var contentHolderRight = GameObject.Instantiate(contentHolderLeft, horizontalContentHolder.transform);
            contentHolderRight.name = "RightContent";
            var verticalLayoutGroupRight = contentHolderRight.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroupRight.childControlHeight = verticalLayoutGroupRight.childControlWidth = false;
            verticalLayoutGroupRight.childForceExpandHeight = verticalLayoutGroupRight.childForceExpandWidth = false;

            var outlineTemp = new GameObject("PFPPrefab", typeof(Image));
            var outlineImage = outlineTemp.GetComponent<Image>();
            outlineImage.maskable = true;
            outlineImage.preserveAspect = true;

            var maskTemp = GameObject.Instantiate(outlineTemp, outlineTemp.transform);
            maskTemp.name = "ImageMask";
            var pfpTemp = GameObject.Instantiate(maskTemp, maskTemp.transform);
            pfpTemp.name = "Image";

            var mask = maskTemp.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var maskImage = maskTemp.GetComponent<Image>();
            maskImage.sprite = AssetManager.GetSprite("PfpMask.png");
            maskTemp.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

            var pfpImage = pfpTemp.GetComponent<Image>();
            outlineTemp.transform.SetSiblingIndex(0);
            outlineImage.enabled = false;
            pfpImage.sprite = AssetManager.GetSprite("icon.png");
            pfpImage.preserveAspect = false;

            var layoutElement = outlineTemp.AddComponent<LayoutElement>();
            layoutElement.minHeight = layoutElement.minWidth = 96;
            var pfp = GameObject.Instantiate(outlineTemp, horizontalContentHolder.transform);
            pfp.transform.SetSiblingIndex(0);
            pfp.name = "PFP";
            GameObject.DestroyImmediate(outlineTemp);

            GameObject.DontDestroyOnLoad(_userCardPrefab);
            _userCardPrefab.SetActive(false);
        }

        #endregion

        #region Create Objects

        public static LoadingIcon CreateLoadingIcon(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, bool isActive, string name) =>
            new LoadingIcon(CreateImageHolder(canvasTransform, position, size, sprite, name), isActive);

        public static GameObject CreateImageHolder(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, string name)
        {
            GameObject imageHolder = new GameObject(name, typeof(Image));
            imageHolder.transform.SetParent(canvasTransform);

            var rect = imageHolder.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = imageHolder.GetComponent<Image>();
            image.preserveAspect = true;
            image.color = Color.white;
            image.sprite = sprite;

            image.gameObject.SetActive(true);

            return imageHolder;
        }

        public static GameObject CreateClickableImageHolder(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, string name, Action onClick)
        {
            var imageHolder = CreateImageHolder(canvasTransform, position, size, sprite, name);
            var eventTrigger = imageHolder.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerClickEvent = new EventTrigger.Entry();
            pointerClickEvent.eventID = EventTriggerType.PointerClick;
            pointerClickEvent.callback.AddListener((data) => onClick?.Invoke());
            eventTrigger.triggers.Add(pointerClickEvent);

            return imageHolder;
        }

        public static ProgressBar CreateProgressBar(Transform canvasTransform, Vector2 position, Vector2 size, bool active, string name)
        {
            Slider slider = GameObject.Instantiate(_settingsPanelVolumeSlider, canvasTransform);
            DestroyFromParent(slider.gameObject, "Handle Slide Area");
            slider.name = name;
            slider.onValueChanged = new Slider.SliderEvent();
            RectTransform rect = slider.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.2f); //line up centered and slightly higher than the lowest point
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.pivot = new Vector2(0.5f, 1f);
            slider.minValue = slider.value = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;
            slider.wholeNumbers = false;
            ProgressBar bar = new ProgressBar(slider, active);
            return bar;
        }

        private static void TintImage(Image image, Color tint, float percent) =>
            image.color = new Color(image.color.r * (1f - percent) + tint.r * percent, image.color.g * (1f - percent) + tint.g * percent, image.color.b * (1f - percent) + tint.b * percent);

        public static GameObject CreateOverlayPanel(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, float borderThiccness, string name)
        {
            GameObject overlayPanel = GameObject.Instantiate(_overlayPanelPrefab, canvasTransform);
            overlayPanel.name = name;
            var fgRect = overlayPanel.transform.Find("FSLatencyPanel/LatencyFG").GetComponent<RectTransform>();
            var bgRect = overlayPanel.transform.Find("FSLatencyPanel/LatencyBG").GetComponent<RectTransform>();
            fgRect.sizeDelta = size;
            fgRect.anchoredPosition = anchoredPosition;
            bgRect.sizeDelta = size + (Vector2.one * borderThiccness);
            bgRect.anchoredPosition = anchoredPosition;
            overlayPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            overlayPanel.SetActive(true);

            return overlayPanel;
        }

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;

            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = 22;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Wrap;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;

            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        public static CustomButton CreateDefaultButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, int fontsize, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = fontsize;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Wrap;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        //Backward Compatibility
        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, Sprite sprite, string name, Action onClick = null)
        => CreateCustomButton(canvasTransform, anchoredPosition, size, sprite, true, name, onClick);

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, Sprite sprite, bool isImageThemable, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;

            GameObject imageHolder = newButton.textHolder.gameObject;
            imageHolder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            imageHolder.transform.localScale = new Vector3(.81f, .81f);
            GameObject.DestroyImmediate(imageHolder.GetComponent<Text>());
            Image image = imageHolder.AddComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.maskable = true;
            image.sprite = sprite;

            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        public static GameObject CreateModifierButton(Transform canvasTransform, Sprite sprite, string name, string description, bool active, Action onClick = null)
        {
            var btn = CreateCustomButton(canvasTransform, new Vector2(350, -200), new Vector2(32, 32), sprite, name, onClick).gameObject;
            if (description != "")
                btn.AddComponent<BubblePopupHandler>().Initialize(CreateBubble(Vector2.zero, $"{name}Bubble", description, 6, true, 16));
            var glow = new GameObject("glow", typeof(Image));
            var image = glow.GetComponent<Image>();
            image.maskable = true;
            image.sprite = AssetManager.GetSprite("glow.png");
            glow.transform.SetParent(btn.transform);
            glow.transform.localScale = Vector3.one / 1.2f;
            glow.SetActive(active);
            image.color = Color.white;
            var rect = btn.GetComponent<RectTransform>();
            rect.pivot = Vector2.one / 2f;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.localScale = Vector2.zero;
            rect.eulerAngles = active ? new Vector3(0, 0, 8) : Vector3.zero;
            return btn;
        }  

        public static TMP_InputField CreateInputField(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string name, bool isPassword)
        {
            TMP_InputField inputField = GameObject.Instantiate(_inputFieldPrefab, canvasTransform);
            inputField.name = name;
            inputField.inputType = isPassword ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;

            //Have to do this so the input text actually updates after we're done typing :derp:
            inputField.onEndEdit.AddListener((text) => inputField.text = text);

            inputField.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
            inputField.GetComponent<RectTransform>().sizeDelta = size;

            return inputField;
        }

        public static Slider CreateVerticalSliderFromPrefab(Transform canvasTransform, string name)
        {
            Slider slider = GameObject.Instantiate(_verticalSliderPrefab, canvasTransform);
            slider.name = name;
            return slider;
        }

        public static Slider CreateSliderFromPrefab(Transform canvasTransform, string name)
        {
            Slider slider = GameObject.Instantiate(_sliderPrefab, canvasTransform);
            slider.name = name;
            return slider;
        }

        public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Color color, TextFont textFont = TextFont.Comfortaa) => CreateSingleText(canvasTransform, name, text, new Vector2(0, 1), canvasTransform.GetComponent<RectTransform>().sizeDelta, color, textFont);
        public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Vector2 pivot, Vector2 size, Color color, TextFont textFont = TextFont.Comfortaa)
        {
            TMP_Text singleText;
            switch (textFont)
            {
                case TextFont.Multicolore:
                    singleText = GameObject.Instantiate(_multicoloreTextPrefab, canvasTransform);
                    break;
                default:
                    singleText = GameObject.Instantiate(_comfortaaTextPrefab, canvasTransform);
                    break;
            }
            singleText.name = name;

            singleText.text = text;
            singleText.color = color;
            singleText.gameObject.GetComponent<RectTransform>().pivot = pivot;
            singleText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
            singleText.enableWordWrapping = true;

            singleText.gameObject.SetActive(true);

            return singleText;
        }

        public static TMP_Text CreateDoubleText(Transform canvasTransform, string name, string text, Color color)
        {
            TMP_Text doubledText = GameObject.Instantiate(_comfortaaTextPrefab, canvasTransform);
            doubledText.name = name;

            doubledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            doubledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            doubledText.text = text;
            doubledText.color = color;

            return doubledText;
        }

        public static TMP_Text CreateTripleText(Transform canvasTransform, string name, string text, Color color)
        {
            TMP_Text tripledText = GameObject.Instantiate(_comfortaaTextPrefab, canvasTransform);
            tripledText.name = name;

            tripledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            tripledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            tripledText.text = text;


            TMP_Text tripledTextSecondLayer = GameObject.Instantiate(_comfortaaTextPrefab, canvasTransform);
            tripledTextSecondLayer.name = name + "SecondLayer";

            tripledTextSecondLayer.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            tripledTextSecondLayer.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 12);
            tripledTextSecondLayer.text = text;
            tripledTextSecondLayer.color = color;

            return tripledText;
        }

        private static string GetTallyBubbleText(int[] tally) =>
            tally != null ? $"Perfect: {tally[4]}\n" +
                            $"Nice: {tally[3]}\n" +
                            $"Okay: {tally[2]}\n" +
                            $"Meh: {tally[1]}\n" +
                            $"Nasty: {tally[0]}\n" : "No Tally";

        public static GameObject CreateBubble(Vector2 size, string name, string text) => CreateBubble(size, name, text, new Vector2(1, 0), 6, false);
        public static GameObject CreateBubble(Vector2 size, string name, string text, int borderThiccness, bool autoResize, int fontSize = 22) => CreateBubble(size, name, text, new Vector2(1, 0), 6, autoResize, fontSize);

        public static GameObject CreateBubble(Vector2 size, string name, string text, Vector2 alignement, int borderThiccness, bool autoResize, int fontSize = 22)
        {
            var bubble = GameObject.Instantiate(_bubblePrefab);
            bubble.name = name;
            bubble.GetComponent<RectTransform>().sizeDelta = size;
            bubble.GetComponent<RectTransform>().pivot = alignement;
            var windowbody = bubble.transform.Find("Window Body");
            windowbody.GetComponent<RectTransform>().sizeDelta = -(Vector2.one * borderThiccness);

            var textObj = windowbody.Find("BubbleText").GetComponent<TMP_Text>();
            textObj.text = text;
            textObj.fontSize = fontSize;
            textObj.fontSizeMax = fontSize;
            textObj.rectTransform.sizeDelta = size;
            textObj.margin = Vector3.one * borderThiccness;

            if (autoResize)
            {
                AddAutoSizeToObject(windowbody.gameObject, borderThiccness);
                AddAutoSizeToObject(bubble, borderThiccness);
            }
            else
                textObj.enableAutoSizing = true;

            return bubble;
        }

        private static void AddAutoSizeToObject(GameObject obj, int padding)
        {
            var contentFitter = obj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vertical = obj.AddComponent<VerticalLayoutGroup>();
            vertical.childForceExpandHeight = vertical.childForceExpandWidth = false;
            vertical.childScaleHeight = vertical.childScaleWidth = false;
            vertical.childControlHeight = vertical.childControlWidth = true;
            vertical.padding = new RectOffset(padding, padding, padding, padding);
        }

        public static GameObject CreateBubble(Vector2 size, string name, Sprite sprite)
        {
            var imageObj = CreateImageHolder(null, Vector2.zero, size, sprite, name);
            imageObj.GetComponent<Image>().maskable = false;
            imageObj.AddComponent<CanvasGroup>().blocksRaycasts = false;
            return imageObj;
        }


        public static TootTallyNotif CreateNotif(Transform canvasTransform, string name, string text, Color textColor)
        {
            TootTallyNotif notif = GameObject.Instantiate(_tootTallyNotifPrefab, canvasTransform);

            notif.name = name;
            notif.SetTextColor(textColor);
            notif.SetText(text);

            return notif;
        }


        #endregion

        public static void DestroyFromParent(GameObject parent, string objectName)
        {
            try
            {
                GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
            }
            catch (Exception)
            {
                Plugin.LogError($"Object {objectName} couldn't be deleted from {parent.name} parent");
            }
        }

        public enum TextFont
        {
            Comfortaa,
            Multicolore
        }
    }
}
