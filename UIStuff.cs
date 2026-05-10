using HarmonyLib;
using LeTai.Asset.TranslucentImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Shapez2Multiplayer
{
    public static class UIStuff
    {
        public static readonly FieldInfo HUDInputFieldUIInputFieldInfo = AccessTools.Field(typeof(HUDInputField), "UIInputField");
        public static readonly FieldInfo HUDInputFieldUIPlaceholderTextInfo = AccessTools.Field(typeof(HUDInputField), "UIPlaceholderText");
        public static readonly FieldInfo HUDScrollContainerUIScrollRectInfo = AccessTools.Field(typeof(HUDScrollContainer), "UIScrollRect");
        private static readonly GameObject Panel = CreatePanel();
        private static readonly HUDButton Button = CreateButton();
        private static readonly TextMeshProUGUI TextPrimary = CreateTextPrimary();
        private static readonly TextMeshProUGUI TextSecondary = CreateTextSecondary();
        private static readonly HUDLocalizedText LocalizedTextPrimary = CreateLocalizedTextPrimary();
        private static readonly HUDLocalizedText LocalizedTextSecondary = CreateLocalizedTextSecondary();
        private static readonly HUDInputField InputField = CreateInputField();
        private static readonly HUDScrollContainer ScrollContainer = CreateScrollContainer();
        private static readonly GameObject Divider = CreateDivider();
        private static GameObject CreatePanel()
        {
            GameObject Panel = new GameObject("Panel");
            GameObject.DontDestroyOnLoad(Panel);
            Panel.SetActiveSelfExt(false);
            Panel.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = Panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            GameObject Translucent = new GameObject("Translucent");
            Translucent.transform.SetParent(Panel.transform);
            Translucent.layer = LayerMask.NameToLayer("UI");
            RectTransform translucentRectTransform = Translucent.AddComponent<RectTransform>();
            translucentRectTransform.anchorMin = Vector2.zero;
            translucentRectTransform.anchorMax = Vector2.one;
            translucentRectTransform.offsetMin = new Vector2(-15.8316f, -15.8316f);
            translucentRectTransform.offsetMax = new Vector2(15.8316f, 15.8316f);
            TranslucentImage translucentImage = Translucent.AddComponent<TranslucentImage>();
            translucentImage.foregroundOpacity = 0;
            translucentImage.vibrancy = 0.5f;
            translucentImage.material = Shapez2Multiplayer.DefaultTranslucent;
            translucentImage.sprite = Shapez2Multiplayer.HUDPrimaryLightPanelMask;
            translucentImage.type = Image.Type.Sliced;
            translucentImage.raycastPadding = Vector4.zero;
            translucentImage.raycastTarget = false;
            Translucent.AddComponent<HUDTranslucentImageWithCameraResultAsImageSource>();
            GameObject Anim = new GameObject("Anim");
            Anim.transform.SetParent(Panel.transform);
            Anim.layer = LayerMask.NameToLayer("UI");
            RectTransform animRectTransform = Anim.AddComponent<RectTransform>();
            animRectTransform.anchorMin = Vector2.zero;
            animRectTransform.anchorMax = Vector2.one;
            animRectTransform.offsetMin = new Vector2(-15.8316f, -15.8316f);
            animRectTransform.offsetMax = new Vector2(15.8316f, 15.8316f);
            Image animImage = Anim.AddComponent<Image>();
            animImage.sprite = Shapez2Multiplayer.HUDPrimaryLightPanelMask;
            animImage.material = Shapez2Multiplayer.UIAnimatedPanelMenuMaterial;
            animImage.type = Image.Type.Sliced;
            animImage.color = new Color(0.1686f, 0.251f, 0.3412f, 0.502f);
            animImage.raycastPadding = new Vector4(15, 15, 15, 15);
            animImage.raycastTarget = false;
            GameObject PanelPanel = new GameObject("Panel");
            PanelPanel.transform.SetParent(Panel.transform);
            PanelPanel.layer = LayerMask.NameToLayer("UI");
            RectTransform panelPanelRectTransform = PanelPanel.AddComponent<RectTransform>();
            panelPanelRectTransform.anchorMin = Vector2.zero;
            panelPanelRectTransform.anchorMax = Vector2.one;
            panelPanelRectTransform.offsetMin = new Vector2(-15.8316f, -15.8316f);
            panelPanelRectTransform.offsetMax = new Vector2(15.8316f, 15.8316f);
            Image panelPanelImage = PanelPanel.AddComponent<Image>();
            panelPanelImage.sprite = Shapez2Multiplayer.HUDPrimaryLightPanel;
            panelPanelImage.material = Shapez2Multiplayer.UISpriteWithMipMapBiasOverride;
            panelPanelImage.type = Image.Type.Sliced;
            panelPanelImage.raycastPadding = new Vector4(15, 15, 15, 15);
            panelPanelImage.raycastTarget = true;
            return Panel;
        }
        private static HUDButton CreateButton()
        {
            GameObject Button = new GameObject("Button");
            GameObject.DontDestroyOnLoad(Button);
            Button.SetActiveSelfExt(false);
            Button.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = Button.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            CanvasGroup canvasGroup = Button.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.ignoreParentGroups = false;
            canvasGroup.interactable = true;
            GameObject Bg = new GameObject("Bg");
            Bg.transform.SetParent(Button.transform);
            Bg.layer = LayerMask.NameToLayer("UI");
            RectTransform bgRectTransform = Bg.AddComponent<RectTransform>();
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = new Vector2(-12.182f, -11.9783f);
            bgRectTransform.offsetMax = new Vector2(12.182f, 11.9783f);
            Image bgImage = Bg.AddComponent<Image>();
            bgImage.sprite = Shapez2Multiplayer.HUDButtonBase;
            bgImage.material = Shapez2Multiplayer.UIAnimatedButtonMaterial;
            bgImage.type = Image.Type.Sliced;
            bgImage.raycastPadding = new Vector4(8, 8, 8, 8);
            bgImage.pixelsPerUnitMultiplier = 1.42f;
            GameObject HoverIndicator = new GameObject("HoverIndicator");
            HoverIndicator.transform.SetParent(Button.transform);
            HoverIndicator.layer = LayerMask.NameToLayer("UI");
            RectTransform hoverIndicatorRectTransform = HoverIndicator.AddComponent<RectTransform>();
            hoverIndicatorRectTransform.anchorMin = Vector2.zero;
            hoverIndicatorRectTransform.anchorMax = Vector2.one;
            hoverIndicatorRectTransform.offsetMin = new Vector2(-15.2858f, -15.0948f);
            hoverIndicatorRectTransform.offsetMax = new Vector2(15.2858f, 15.0948f);
            Image hoverIndicatorImage = HoverIndicator.AddComponent<Image>();
            hoverIndicatorImage.sprite = Shapez2Multiplayer.HUDButtonHover;
            hoverIndicatorImage.type = Image.Type.Sliced;
            hoverIndicatorImage.raycastPadding = Vector4.zero;
            hoverIndicatorImage.raycastTarget = false;
            hoverIndicatorImage.pixelsPerUnitMultiplier = 1.2f;
            CanvasGroup hoverIndicatorCanvasGroup = HoverIndicator.AddComponent<CanvasGroup>();
            hoverIndicatorCanvasGroup.alpha = 0;
            hoverIndicatorCanvasGroup.blocksRaycasts = false;
            hoverIndicatorCanvasGroup.ignoreParentGroups = false;
            hoverIndicatorCanvasGroup.interactable = false;
            GameObject Text = new GameObject("Text");
            Text.transform.SetParent(Button.transform);
            Text.layer = LayerMask.NameToLayer("UI");
            RectTransform textRectTransform = Text.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = new Vector2(10.307f, 0);
            textRectTransform.offsetMax = new Vector2(-10.307f, 0);
            TextMeshProUGUI textTMP = Text.AddComponent<TextMeshProUGUI>();
            SetupPrimaryText(textTMP);
            HUDLocalizedText hudLocalizedText = Text.AddComponent<HUDLocalizedText>();
            Shapez2Multiplayer.HUDLocalizedTextUITextInfo.SetValue(hudLocalizedText, textTMP);
            Button buttonButton = Button.AddComponent<Button>();
            buttonButton.image = bgImage;
            HUDButton hudButton = Button.AddComponent<HUDButton>();
            Shapez2Multiplayer.HUDButtonUIButtonInfo.SetValue(hudButton, buttonButton);
            Shapez2Multiplayer.HUDButtonUITextInfo.SetValue(hudButton, hudLocalizedText);
            Shapez2Multiplayer.HUDButtonUIMainGroupInfo.SetValue(hudButton, canvasGroup);
            Shapez2Multiplayer.HUDButtonUIHoverIndicatorGroupInfo.SetValue(hudButton, hoverIndicatorCanvasGroup);
            Shapez2Multiplayer.HUDButtonUIMainTransformInfo.SetValue(hudButton, rectTransform);
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(hudButton, new HUDComponent[] { hudLocalizedText });
            return hudButton;
        }
        private static TextMeshProUGUI CreateTextPrimary()
        {
            GameObject Text = new GameObject("Text");
            GameObject.DontDestroyOnLoad(Text);
            Text.SetActiveSelfExt(false);
            Text.layer = LayerMask.NameToLayer("UI");
            TextMeshProUGUI textTMP = Text.AddComponent<TextMeshProUGUI>();
            SetupPrimaryText(textTMP);
            return textTMP;
        }
        private static TextMeshProUGUI CreateTextSecondary()
        {
            GameObject Text = new GameObject("Text");
            GameObject.DontDestroyOnLoad(Text);
            Text.SetActiveSelfExt(false);
            Text.layer = LayerMask.NameToLayer("UI");
            TextMeshProUGUI textTMP = Text.AddComponent<TextMeshProUGUI>();
            SetupSecondaryText(textTMP);
            return textTMP;
        }
        private static HUDLocalizedText CreateLocalizedTextPrimary()
        {
            GameObject Text = new GameObject("Text");
            GameObject.DontDestroyOnLoad(Text);
            Text.SetActiveSelfExt(false);
            Text.layer = LayerMask.NameToLayer("UI");
            TextMeshProUGUI textTMP = Text.AddComponent<TextMeshProUGUI>();
            SetupPrimaryText(textTMP);
            HUDLocalizedText hudLocalizedText = Text.AddComponent<HUDLocalizedText>();
            Shapez2Multiplayer.HUDLocalizedTextUITextInfo.SetValue(hudLocalizedText, textTMP);
            return hudLocalizedText;
        }
        private static HUDLocalizedText CreateLocalizedTextSecondary()
        {
            GameObject Text = new GameObject("Text");
            GameObject.DontDestroyOnLoad(Text);
            Text.SetActiveSelfExt(false);
            Text.layer = LayerMask.NameToLayer("UI");
            TextMeshProUGUI textTMP = Text.AddComponent<TextMeshProUGUI>();
            SetupSecondaryText(textTMP);
            HUDLocalizedText hudLocalizedText = Text.AddComponent<HUDLocalizedText>();
            Shapez2Multiplayer.HUDLocalizedTextUITextInfo.SetValue(hudLocalizedText, textTMP);
            return hudLocalizedText;
        }
        private static HUDInputField CreateInputField()
        {
            GameObject InputField = new GameObject("InputField");
            GameObject.DontDestroyOnLoad(InputField);
            InputField.SetActiveSelfExt(false);
            InputField.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = InputField.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            TMP_InputField tmp_InputField = InputField.AddComponent<TMP_InputField>();
            UniformModifier uniformModifier = InputField.AddComponent<UniformModifier>();
            ProceduralImage proceduralImage = InputField.AddComponent<ProceduralImage>();
            tmp_InputField.image = proceduralImage;
            uniformModifier.Radius = 12;
            GameObject TextArea = new GameObject("Text Area");
            TextArea.transform.SetParent(InputField.transform);
            TextArea.layer = LayerMask.NameToLayer("UI");
            RectTransform TextAreaRectTransform = TextArea.AddComponent<RectTransform>();
            TextAreaRectTransform.anchorMin = Vector2.zero;
            TextAreaRectTransform.anchorMax = Vector2.one;
            TextAreaRectTransform.offsetMin = new Vector2(10, 6);
            TextAreaRectTransform.offsetMax = new Vector2(-10, -7);
            TextAreaRectTransform.anchoredPosition = new Vector2(0, -0.5f);
            RectMask2D rectMask2D = TextArea.AddComponent<RectMask2D>();
            rectMask2D.padding = new Vector4(-8, -5, -8, -5);
            GameObject Placeholder = new GameObject("Placeholder");
            Placeholder.transform.SetParent(TextArea.transform);
            Placeholder.layer = LayerMask.NameToLayer("UI");
            RectTransform placeholderRectTransform = Placeholder.AddComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.offsetMin = Vector2.zero;
            placeholderRectTransform.offsetMax = Vector2.zero;
            LayoutElement PlaceholderLayoutElement = Placeholder.AddComponent<LayoutElement>();
            PlaceholderLayoutElement.ignoreLayout = true;
            TextMeshProUGUI placeholderTextMeshProUGUI = Placeholder.AddComponent<TextMeshProUGUI>();
            tmp_InputField.placeholder = placeholderTextMeshProUGUI;
            placeholderTextMeshProUGUI.alignment = TextAlignmentOptions.Left;
            placeholderTextMeshProUGUI.color = new Color(1, 1, 1, 0.098f);
            placeholderTextMeshProUGUI.extraPadding = true;
            placeholderTextMeshProUGUI.fontFeatures = new List<UnityEngine.TextCore.OTL_FeatureTag> { UnityEngine.TextCore.OTL_FeatureTag.kern };
            placeholderTextMeshProUGUI.font = Shapez2Multiplayer.FontLightSDF;
            placeholderTextMeshProUGUI.fontSize = 20;
            placeholderTextMeshProUGUI.fontSizeMax = 72;
            placeholderTextMeshProUGUI.fontSizeMin = 18;
            placeholderTextMeshProUGUI.isOrthographic = true;
            HUDLocalizedText hudLocalizedText = Placeholder.AddComponent<HUDLocalizedText>();
            Shapez2Multiplayer.HUDLocalizedTextUITextInfo.SetValue(hudLocalizedText, placeholderTextMeshProUGUI);
            GameObject Text = new GameObject("Text");
            Text.transform.SetParent(TextArea.transform);
            Text.layer = LayerMask.NameToLayer("UI");
            RectTransform textRectTransform = Text.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
            TextMeshProUGUI TextMeshProUGUI = Text.AddComponent<TextMeshProUGUI>();
            tmp_InputField.textComponent = TextMeshProUGUI;
            TextMeshProUGUI.alignment = TextAlignmentOptions.Left;
            TextMeshProUGUI.extraPadding = true;
            TextMeshProUGUI.fontFeatures = new List<UnityEngine.TextCore.OTL_FeatureTag> { UnityEngine.TextCore.OTL_FeatureTag.kern };
            TextMeshProUGUI.font = Shapez2Multiplayer.FontLightSDF;
            TextMeshProUGUI.fontSize = 20;
            TextMeshProUGUI.fontSizeMax = 72;
            TextMeshProUGUI.fontSizeMin = 18;
            TextMeshProUGUI.isOrthographic = true;
            tmp_InputField.textViewport = TextAreaRectTransform;
            GameObject Border = new GameObject("Border");
            Border.transform.SetParent(InputField.transform);
            Border.layer = LayerMask.NameToLayer("UI");
            RectTransform borderRectTransform = Border.AddComponent<RectTransform>();
            borderRectTransform.anchorMin = Vector2.zero;
            borderRectTransform.anchorMax = Vector2.one;
            borderRectTransform.offsetMin = Vector2.zero;
            borderRectTransform.offsetMax = Vector2.zero;
            UniformModifier borderUniformModifier = Border.AddComponent<UniformModifier>();
            ProceduralImage borderProceduralImage = Border.AddComponent<ProceduralImage>();
            borderProceduralImage.BorderWidth = 1.5f;
            borderProceduralImage.color = new Color(1, 1, 1, 0.0314f);
            borderProceduralImage.raycastTarget = false;
            borderUniformModifier.Radius = 12;
            tmp_InputField.characterLimit = 128;
            tmp_InputField.characterValidation = TMP_InputField.CharacterValidation.Regex;
            tmp_InputField.contentType = TMP_InputField.ContentType.Custom;
            tmp_InputField.fontAsset = Shapez2Multiplayer.FontLightSDF;
            tmp_InputField.keyboardType = TouchScreenKeyboardType.ASCIICapable;
            tmp_InputField.pointSize = 20;
            tmp_InputField.richText = false;
            tmp_InputField.selectionColor = new Color(1, 0.6167f, 0, 0.9686f);
            tmp_InputField.colors = new ColorBlock()
            {
                colorMultiplier = 1,
                disabledColor = new Color(0, 0, 0, 0.0314f),
                fadeDuration = 0.1f,
                highlightedColor = new Color(0, 0, 0, 0.549f),
                normalColor = new Color(0, 0, 0, 0.349f),
                pressedColor = new Color(0, 0, 0, 0.549f),
                selectedColor = new Color(0, 0, 0, 0.451f)
            };
            Shapez2Multiplayer.tmpInputFieldRegexValue.SetValue(tmp_InputField, "^([^\\n\\r])$");
            HUDInputField hudInputField = InputField.AddComponent<HUDInputField>();
            HUDInputFieldUIInputFieldInfo.SetValue(hudInputField, tmp_InputField);
            HUDInputFieldUIPlaceholderTextInfo.SetValue(hudInputField, hudLocalizedText);
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(hudInputField, new HUDComponent[] { hudLocalizedText });
            return hudInputField;
        }
        public static HUDScrollContainer CreateScrollContainer()
        {
            GameObject ScrollContainer = new GameObject("ScrollContainer");
            GameObject.DontDestroyOnLoad(ScrollContainer);
            ScrollContainer.SetActiveSelfExt(false);
            ScrollContainer.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = ScrollContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            ScrollRect scrollRect = ScrollContainer.AddComponent<ScrollRect>();
            GameObject Viewport = new GameObject("Viewport");
            Viewport.transform.SetParent(ScrollContainer.transform);
            Viewport.layer = LayerMask.NameToLayer("UI");
            RectTransform viewportRectTransform = Viewport.AddComponent<RectTransform>();
            viewportRectTransform.pivot = Vector2.up;
            viewportRectTransform.anchorMin = Vector2.zero;
            viewportRectTransform.anchorMax = Vector2.one;
            viewportRectTransform.offsetMin = Vector2.zero;
            viewportRectTransform.offsetMax = Vector2.zero;
            Image viewportImage = Viewport.AddComponent<Image>();
            viewportImage.sprite = Shapez2Multiplayer.HUDPrimaryLightPanelMask;
            viewportImage.type = Image.Type.Sliced;
            Mask viewportMask = Viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            GameObject Content = new GameObject("Content");
            Content.transform.SetParent(Viewport.transform);
            Content.layer = LayerMask.NameToLayer("UI");
            RectTransform contentRectTransform = Content.AddComponent<RectTransform>();
            contentRectTransform.pivot = Vector2.up;
            contentRectTransform.anchorMin = Vector2.up;
            contentRectTransform.anchorMax = Vector2.one;
            contentRectTransform.offsetMin = new Vector2(0, -50);
            contentRectTransform.offsetMax = Vector2.zero;
            // Add graphicraycaster to Content after Instantiated with blockingMask 55
            VerticalLayoutGroup contentVerticalLayoutGroup = Content.AddComponent<VerticalLayoutGroup>();
            contentVerticalLayoutGroup.childScaleHeight = true;
            contentVerticalLayoutGroup.childScaleWidth = true;
            contentVerticalLayoutGroup.spacing = 10;
            contentVerticalLayoutGroup.padding = new RectOffset(25, 25, 25, 25);
            ContentSizeFitter contentContentSizeFitter = Content.AddComponent<ContentSizeFitter>();
            contentContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            GameObject ScrollbarHorizontal = new GameObject("Scrollbar Horizontal");
            ScrollbarHorizontal.transform.SetParent(ScrollContainer.transform);
            ScrollbarHorizontal.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarHorizontalRectTransform = ScrollbarHorizontal.AddComponent<RectTransform>();
            Image scrollbarHorizontalImage = ScrollbarHorizontal.AddComponent<Image>();
            scrollbarHorizontalImage.type = Image.Type.Sliced;
            scrollbarHorizontalImage.color = Color.clear;
            GameObject ScrollbarHorizontalSlidingArea = new GameObject("Sliding Area");
            ScrollbarHorizontalSlidingArea.transform.SetParent(ScrollbarHorizontal.transform);
            ScrollbarHorizontalSlidingArea.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarHorizontalSlidingAreaRectTransform = ScrollbarHorizontalSlidingArea.AddComponent<RectTransform>();
            GameObject ScrollbarHorizontalHandle = new GameObject("Handle");
            ScrollbarHorizontalHandle.transform.SetParent(ScrollbarHorizontalSlidingArea.transform);
            ScrollbarHorizontalHandle.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarHorizontalHandleRectTransform = ScrollbarHorizontalHandle.AddComponent<RectTransform>();
            scrollbarHorizontalHandleRectTransform.anchorMin = Vector2.zero;
            scrollbarHorizontalHandleRectTransform.anchorMax = Vector2.one;
            scrollbarHorizontalHandleRectTransform.offsetMin = new Vector2(-3, -3);
            scrollbarHorizontalHandleRectTransform.offsetMax = new Vector2(3, 3);
            Image scrollbarHorizontalHandleImage = ScrollbarHorizontalHandle.AddComponent<Image>();
            scrollbarHorizontalHandleImage.sprite = Shapez2Multiplayer.HUDScrollbarPanelBg;
            scrollbarHorizontalHandleImage.type = Image.Type.Sliced;
            scrollbarHorizontalHandleImage.pixelsPerUnitMultiplier = 7;
            scrollbarHorizontalHandleImage.raycastPadding = new Vector4(-5, -5, -5, -5);
            Scrollbar scrollbarHorizontalScrollbar = ScrollbarHorizontal.AddComponent<Scrollbar>();
            scrollbarHorizontalScrollbar.handleRect = scrollbarHorizontalHandleRectTransform;
            scrollbarHorizontalScrollbar.targetGraphic = scrollbarHorizontalHandleImage;
            scrollbarHorizontalScrollbar.colors = new ColorBlock()
            {
                normalColor = new Color(0.9198f, 0.9236f, 1, 0.0353f),
                highlightedColor = new Color(0.9216f, 0.9255f, 1, 0.1059f),
                pressedColor = new Color(0.9216f, 0.9255f, 1, 0.1647f),
                selectedColor = new Color(0.9216f, 0.9255f, 1, 0.149f),
                disabledColor = new Color(0.9216f, 0.9255f, 1, 0.0078f),
                colorMultiplier = 1
            };
            GameObject ScrollbarVertical = new GameObject("Scrollbar Vertical");
            ScrollbarVertical.transform.SetParent(ScrollContainer.transform);
            ScrollbarVertical.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarVerticalRectTransform = ScrollbarVertical.AddComponent<RectTransform>();
            Image scrollbarVerticalImage = ScrollbarVertical.AddComponent<Image>();
            scrollbarVerticalImage.type = Image.Type.Sliced;
            scrollbarVerticalImage.color = Color.clear;
            GameObject ScrollbarVerticalSlidingArea = new GameObject("Sliding Area");
            ScrollbarVerticalSlidingArea.transform.SetParent(ScrollbarVertical.transform);
            ScrollbarVerticalSlidingArea.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarVerticalSlidingAreaRectTransform = ScrollbarVerticalSlidingArea.AddComponent<RectTransform>();
            GameObject ScrollbarVerticalHandle = new GameObject("Handle");
            ScrollbarVerticalHandle.transform.SetParent(ScrollbarVerticalSlidingArea.transform);
            ScrollbarVerticalHandle.layer = LayerMask.NameToLayer("UI");
            RectTransform scrollbarVerticalHandleRectTransform = ScrollbarVerticalHandle.AddComponent<RectTransform>();
            scrollbarVerticalHandleRectTransform.anchorMin = Vector2.zero;
            scrollbarVerticalHandleRectTransform.anchorMax = Vector2.one;
            scrollbarVerticalHandleRectTransform.offsetMin = new Vector2(-3, -3);
            scrollbarVerticalHandleRectTransform.offsetMax = new Vector2(3, 3);
            Image scrollbarVerticalHandleImage = ScrollbarVerticalHandle.AddComponent<Image>();
            scrollbarVerticalHandleImage.sprite = Shapez2Multiplayer.HUDScrollbarPanelBg;
            scrollbarVerticalHandleImage.type = Image.Type.Sliced;
            scrollbarVerticalHandleImage.pixelsPerUnitMultiplier = 7;
            scrollbarVerticalHandleImage.raycastPadding = new Vector4(-5, -5, -5, -5);
            Scrollbar scrollbarVerticalScrollbar = ScrollbarVertical.AddComponent<Scrollbar>();
            scrollbarVerticalScrollbar.handleRect = scrollbarVerticalHandleRectTransform;
            scrollbarVerticalScrollbar.targetGraphic = scrollbarVerticalHandleImage;
            scrollbarVerticalScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);
            scrollbarVerticalScrollbar.colors = new ColorBlock()
            {
                normalColor = new Color(0.9198f, 0.9236f, 1, 0.0353f),
                highlightedColor = new Color(0.9216f, 0.9255f, 1, 0.1059f),
                pressedColor = new Color(0.9216f, 0.9255f, 1, 0.1647f),
                selectedColor = new Color(0.9216f, 0.9255f, 1, 0.149f),
                disabledColor = new Color(0.9216f, 0.9255f, 1, 0.0078f),
                colorMultiplier = 1
            };
            scrollRect.content = contentRectTransform;
            scrollRect.viewport = viewportRectTransform;
            scrollRect.horizontalScrollbar = scrollbarHorizontalScrollbar;
            scrollRect.verticalScrollbar = scrollbarVerticalScrollbar;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3f;
            scrollRect.verticalScrollbarSpacing = -3f;
            scrollRect.decelerationRate = 0.01f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.normalizedPosition = Vector2.right;
            scrollbarHorizontalRectTransform.pivot = Vector2.zero;
            scrollbarHorizontalRectTransform.anchorMin = Vector2.zero;
            scrollbarHorizontalRectTransform.anchorMax = Vector2.right;
            scrollbarHorizontalRectTransform.offsetMin = Vector2.zero;
            scrollbarHorizontalRectTransform.offsetMax = new Vector2(0, 20);
            scrollbarHorizontalSlidingAreaRectTransform.anchorMin = Vector2.zero;
            scrollbarHorizontalSlidingAreaRectTransform.anchorMax = Vector2.one;
            scrollbarHorizontalSlidingAreaRectTransform.anchoredPosition = new Vector2(0, 2.5f);
            scrollbarHorizontalSlidingAreaRectTransform.sizeDelta = new Vector2(-60, -15);
            scrollbarVerticalRectTransform.pivot = Vector2.one;
            scrollbarVerticalRectTransform.anchorMin = Vector2.right;
            scrollbarVerticalRectTransform.anchorMax = Vector2.one;
            scrollbarVerticalRectTransform.offsetMin = new Vector2(-20, 0);
            scrollbarVerticalRectTransform.offsetMax = Vector2.zero;
            scrollbarVerticalSlidingAreaRectTransform.anchorMin = Vector2.zero;
            scrollbarVerticalSlidingAreaRectTransform.anchorMax = Vector2.one;
            scrollbarVerticalSlidingAreaRectTransform.anchoredPosition = new Vector2(-2.5f, 0);
            scrollbarVerticalSlidingAreaRectTransform.sizeDelta = new Vector2(-15, -60);
            HUDScrollContainer hudScrollContainer = ScrollContainer.AddComponent<HUDScrollContainer>();
            HUDScrollContainerUIScrollRectInfo.SetValue(hudScrollContainer, scrollRect);
            return hudScrollContainer;
        }
        public static GameObject CreateDivider()
        {
            GameObject Divider = new GameObject("Divider");
            GameObject.DontDestroyOnLoad(Divider);
            Divider.SetActiveSelfExt(false);
            Divider.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = Divider.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(0, -2.5f);
            rectTransform.offsetMax = new Vector2(0, 1.5f);
            Image Image = Divider.AddComponent<Image>();
            Image.sprite = Shapez2Multiplayer.HUDAntiAliasedHorizontalScrollDivider;
            Image.material = Shapez2Multiplayer.UISpriteWithMipMapBiasOverride;
            Image.type = Image.Type.Sliced;
            Image.raycastTarget = false;
            return Divider;
        }
        public static void SetupPrimaryText(TextMeshProUGUI text)
        {
            text.color = new Color(1, 1, 1, 0.749f);
            text.enableAutoSizing = true;
            text.fontFeatures = new List<UnityEngine.TextCore.OTL_FeatureTag> { UnityEngine.TextCore.OTL_FeatureTag.kern };
            text.font = Shapez2Multiplayer.FontMediumSDF;
            text.fontSize = 20;
            text.fontSizeMax = 20;
            text.fontSizeMin = 12;
            text.fontStyle = FontStyles.UpperCase;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.isOrthographic = true;
            text.margin = new Vector4(0, 0, -0.0001f, 0);
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.raycastTarget = false;
        }
        public static void SetupSecondaryText(TextMeshProUGUI text)
        {
            text.color = new Color(1, 1, 1, 0.502f);
            text.enableAutoSizing = true;
            text.font = Shapez2Multiplayer.FontLightSDF;
            text.fontSize = 20;
            text.fontSizeMax = 20;
            text.fontSizeMin = 12;
            text.fontStyle = FontStyles.Normal;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.isOrthographic = true;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.raycastTarget = false;
        }
        public static GameObject AddPanel(Transform target, HUDComponent targetComponent, bool ConstructNow = false)
        {
            GameObject panel = GameObject.Instantiate(Panel, target, false);
            panel.transform.localScale = Vector3.one;
            panel.SetActiveSelfExt(true);
            var component = panel.transform.GetChild(0).GetComponent<HUDTranslucentImageWithCameraResultAsImageSource>();
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDTranslucentImageWithCameraResultAsImageSource)).Invoke(targetComponent, new object[] { component });
            return panel;
        }
        public static HUDButton AddButton(Transform target, HUDComponent targetComponent, bool ConstructNow = false, bool secondary = false)
        {
            GameObject button = GameObject.Instantiate(Button.gameObject, target, false);
            button.transform.localScale = Vector3.one;
            button.SetActiveSelfExt(true);
            var component = button.GetComponent<HUDButton>();
            if (secondary) button.transform.GetChild(0).GetComponent<Image>().sprite = Shapez2Multiplayer.HUDSecondaryButtonBase;
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDButton)).Invoke(targetComponent, new object[] { component });
            return component;
        }
        public static TextMeshProUGUI AddTextPrimary(Transform target)
        {
            GameObject text = GameObject.Instantiate(TextPrimary.gameObject, target, false);
            text.transform.localScale = Vector3.one;
            text.SetActiveSelfExt(true);
            return text.GetComponent<TextMeshProUGUI>();
        }
        public static TextMeshProUGUI AddTextSecondary(Transform target)
        {
            GameObject text = GameObject.Instantiate(TextSecondary.gameObject, target, false);
            text.transform.localScale = Vector3.one;
            text.SetActiveSelfExt(true);
            return text.GetComponent<TextMeshProUGUI>();
        }
        public static HUDLocalizedText AddLocalizedTextPrimary(Transform target, HUDComponent targetComponent, bool ConstructNow = false)
        {
            GameObject localizedText = GameObject.Instantiate(LocalizedTextPrimary.gameObject, target, false);
            localizedText.transform.localScale = Vector3.one;
            localizedText.SetActiveSelfExt(true);
            var component = localizedText.GetComponent<HUDLocalizedText>();
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDLocalizedText)).Invoke(targetComponent, new object[] { component });
            return component;
        }
        public static HUDLocalizedText AddLocalizedTextSecondary(Transform target, HUDComponent targetComponent, bool ConstructNow = false)
        {
            GameObject localizedText = GameObject.Instantiate(LocalizedTextSecondary.gameObject, target, false);
            localizedText.transform.localScale = Vector3.one;
            localizedText.SetActiveSelfExt(true);
            var component = localizedText.GetComponent<HUDLocalizedText>();
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDLocalizedText)).Invoke(targetComponent, new object[] { component });
            return component;
        }
        public static HUDInputField AddInputField(Transform target, HUDComponent targetComponent, bool ConstructNow = false)
        {
            GameObject inputField = GameObject.Instantiate(InputField.gameObject, target, false);
            inputField.transform.localScale = Vector3.one;
            inputField.SetActiveSelfExt(true);
            var component = inputField.GetComponent<HUDInputField>();
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDInputField)).Invoke(targetComponent, new object[] { component });
            return component;
        }
        public static HUDScrollContainer AddScrollContainer(Transform target, HUDComponent targetComponent, bool ConstructNow = false)
        {
            GameObject scrollContainer = GameObject.Instantiate(ScrollContainer.gameObject, target, false);
            scrollContainer.transform.localScale = Vector3.one;
            scrollContainer.SetActiveSelfExt(true);
            scrollContainer.GetComponent<ScrollRect>().content.gameObject.AddComponent<GraphicRaycaster>().blockingMask = 55;
            var component = scrollContainer.GetComponent<HUDScrollContainer>();
            Shapez2Multiplayer.componentChildComponentReferences.SetValue(targetComponent, ((HUDComponent[])Shapez2Multiplayer.componentChildComponentReferences.GetValue(targetComponent)).Concat(new HUDComponent[] { component }).ToArray());
            if (ConstructNow) Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDScrollContainer)).Invoke(targetComponent, new object[] { component });
            return component;
        }
        public static GameObject AddDivider(Transform target, bool bottom = false)
        {
            GameObject divider = GameObject.Instantiate(Divider, target, false);
            divider.transform.localScale = Vector3.one;
            if (bottom)
            {
                RectTransform rectTransform = divider.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.right;
            }
            divider.SetActiveSelfExt(true);
            return divider;
        }
    }
}
