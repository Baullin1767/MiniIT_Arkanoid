using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.ARKANOID
{
    public sealed class SettingsWindow : MonoBehaviour, IWindow
    {
        [SerializeField]
        private Sprite panelSprite = null;

        [SerializeField]
        private Sprite actionButtonSprite = null;

        [SerializeField]
        private Sprite closeButtonSprite = null;

        [SerializeField]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.45f);

        [SerializeField]
        private Color labelColor = new Color(0.32f, 0.16f, 0.04f, 1f);

        [SerializeField]
        private Vector2 panelSize = new Vector2(900f, 520f);

        [SerializeField]
        private Vector2 buttonSize = new Vector2(340f, 96f);

        private Action leaderBoardAction = null;
        private Action aboutAction = null;
        private Action privacyPolicyAction = null;
        private Action closeAction = null;

        private GameObject runtimeRoot = null;
        private bool isBuilt = false;

        private void Awake()
        {
            EnsureUi();
            Hide();
        }

        public void Configure(Action leaderBoardAction, Action aboutAction, Action privacyPolicyAction, Action closeAction)
        {
            this.leaderBoardAction = leaderBoardAction;
            this.aboutAction = aboutAction;
            this.privacyPolicyAction = privacyPolicyAction;
            this.closeAction = closeAction;
        }

        public void Show()
        {
            // EnsureUi();
            if (runtimeRoot == null)
            {
                return;
            }

            runtimeRoot.SetActive(true);
            runtimeRoot.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            if (runtimeRoot != null)
            {
                runtimeRoot.SetActive(false);
            }
        }

        private void EnsureUi()
        {
            if (isBuilt)
            {
                return;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            runtimeRoot = new GameObject("SettingsRuntimeRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            runtimeRoot.transform.SetParent(canvas.transform, false);
            runtimeRoot.layer = canvas.gameObject.layer;

            RectTransform rootRect = runtimeRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image overlayImage = runtimeRoot.GetComponent<Image>();
            overlayImage.color = overlayColor;
            overlayImage.raycastTarget = true;

            RectTransform panelRect = CreatePanel(rootRect);
            CreateTitle(panelRect, "Settings");
            CreateActionButton(panelRect, "Leaderboard", new Vector2(0f, 70f), actionButtonSprite, () => OpenNestedWindow(leaderBoardAction));
            CreateActionButton(panelRect, "About", new Vector2(0f, -40f), actionButtonSprite, () => OpenNestedWindow(aboutAction));
            CreateActionButton(panelRect, "Privacy Policy", new Vector2(0f, -150f), actionButtonSprite, () => OpenNestedWindow(privacyPolicyAction));
            CreateActionButton(panelRect, "Close", new Vector2(0f, -250f), closeButtonSprite != null ? closeButtonSprite : actionButtonSprite, OnCloseRequested);

            isBuilt = true;
        }

        private RectTransform CreatePanel(RectTransform parent)
        {
            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.layer = parent.gameObject.layer;

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = panelSize;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = panelSprite;
            panelImage.type = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            panelImage.color = Color.white;

            return panelRect;
        }

        private void CreateTitle(RectTransform parent, string value)
        {
            GameObject title = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            title.transform.SetParent(parent, false);
            title.layer = parent.gameObject.layer;

            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(420f, 72f);
            titleRect.anchoredPosition = new Vector2(0f, -55f);

            TextMeshProUGUI text = title.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 40f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = labelColor;
        }

        private void CreateActionButton(RectTransform parent, string label, Vector2 anchoredPosition, Sprite sprite, Action callback)
        {
            GameObject buttonObject = new GameObject(label.Replace(" ", string.Empty) + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.layer = parent.gameObject.layer;

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = buttonSize;
            buttonRect.anchoredPosition = anchoredPosition;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.sprite = sprite;
            buttonImage.color = Color.white;
            buttonImage.type = Image.Type.Simple;
            buttonImage.preserveAspect = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() => callback?.Invoke());

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonRect, false);
            labelObject.layer = parent.gameObject.layer;

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = labelColor;
        }

        private void OpenNestedWindow(Action action)
        {
            action?.Invoke();
        }

        private void OnCloseRequested()
        {
            closeAction?.Invoke();
        }
    }
}
