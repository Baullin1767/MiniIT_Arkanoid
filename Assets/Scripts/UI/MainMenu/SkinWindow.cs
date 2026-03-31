using System;
using System.Collections.Generic;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiniIT.ARKANOID
{
    public class SkinWindow : MonoBehaviour, IWindow
    {
        private const string PlayerSkinKey = "PlayerSkin";

        [SerializeField]
        private Button[] skinButtons = Array.Empty<Button>();

        [SerializeField]
        private UIButton closeButton = null;

        [SerializeField]
        private RectTransform contentRoot = null;

        [SerializeField]
        private Button skinButtonTemplate = null;

        [SerializeField]
        private Sprite[] skinSprites = Array.Empty<Sprite>();

        [SerializeField]
        private int columns = 4;

        [SerializeField]
        private Vector2 layoutPadding = new Vector2(48f, 140f);

        [SerializeField]
        private Vector2 cellSpacing = new Vector2(16f, 18f);

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color unselectedColor = Color.gray;

        private UnityAction[] skinClickHandlers = Array.Empty<UnityAction>();

        private UIView view = null;
        private Action closeCallback = null;
        private UnityAction closeButtonHandler = null;
        private readonly List<Button> runtimeButtons = new List<Button>();

        private void Awake()
        {
            view = GetComponent<UIView>();
            closeButtonHandler = OnCloseClicked;

            if (view != null)
            {
                view.Hide();
            }

            BuildSkinButtons();
            BuildHandlers();
            RefreshSelection();
        }

        private void OnEnable()
        {
            BindCloseButton();
            BindSkinButtons();
        }

        private void OnDisable()
        {
            UnbindCloseButton();
            UnbindSkinButtons();
        }

        public void Show()
        {
            RefreshSelection();
            view?.Show();
        }

        public void Hide()
        {
            view?.Hide();
        }

        public void SetCloseCallback(Action callback)
        {
            closeCallback = callback;
        }

        private void BuildSkinButtons()
        {
            if (skinButtonTemplate == null && skinButtons != null && skinButtons.Length > 0)
            {
                skinButtonTemplate = skinButtons[0];
            }

            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }

            if (skinButtonTemplate == null || contentRoot == null)
            {
                return;
            }

            Sprite[] availableSprites = GetAvailableSprites();
            if (availableSprites.Length == 0)
            {
                return;
            }

            runtimeButtons.Clear();

            skinButtonTemplate.transform.SetParent(contentRoot, false);
            skinButtonTemplate.gameObject.SetActive(true);
            runtimeButtons.Add(skinButtonTemplate);

            if (skinButtons != null)
            {
                foreach (Button button in skinButtons)
                {
                    if (button == null || button == skinButtonTemplate)
                    {
                        continue;
                    }

                    button.gameObject.SetActive(false);
                }
            }

            int totalColumns = Mathf.Max(1, columns);
            int totalRows = Mathf.CeilToInt(availableSprites.Length / (float)totalColumns);
            Rect contentRect = contentRoot.rect;
            float usableWidth = Mathf.Max(240f, contentRect.width - (layoutPadding.x * 2f));
            float usableHeight = Mathf.Max(240f, contentRect.height - (layoutPadding.y * 2f));
            float cellWidth = Mathf.Min(140f, (usableWidth - (cellSpacing.x * (totalColumns - 1))) / totalColumns);
            float cellHeight = Mathf.Min(140f, (usableHeight - (cellSpacing.y * (Mathf.Max(1, totalRows) - 1))) / Mathf.Max(1, totalRows));
            float startX = -((totalColumns - 1) * (cellWidth + cellSpacing.x)) * 0.5f;
            float startY = ((Mathf.Max(1, totalRows) - 1) * (cellHeight + cellSpacing.y)) * 0.5f - 24f;

            for (int i = 0; i < availableSprites.Length; i++)
            {
                Button button = i == 0 ? skinButtonTemplate : Instantiate(skinButtonTemplate, contentRoot);
                button.gameObject.name = $"Skin_{i:00}";
                button.gameObject.SetActive(true);

                RectTransform rectTransform = button.transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);

                    int row = i / totalColumns;
                    int column = i % totalColumns;
                    rectTransform.anchoredPosition = new Vector2(
                        startX + (column * (cellWidth + cellSpacing.x)),
                        startY - (row * (cellHeight + cellSpacing.y)));
                }

                Image image = button.image != null ? button.image : button.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = availableSprites[i];
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;
                }

                if (i > 0)
                {
                    runtimeButtons.Add(button);
                }
            }

            skinButtons = runtimeButtons.ToArray();
        }

        private void BuildHandlers()
        {
            if (skinButtons == null || skinButtons.Length == 0)
            {
                skinClickHandlers = Array.Empty<UnityAction>();
                return;
            }

            Array.Resize(ref skinClickHandlers, skinButtons.Length);
            for (int i = 0; i < skinButtons.Length; i++)
            {
                int index = i;
                skinClickHandlers[i] = () => OnSkinClicked(index);
            }
        }

        private void BindCloseButton()
        {
            if (closeButton == null)
            {
                return;
            }

            closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
            closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(closeButtonHandler);
        }

        private void UnbindCloseButton()
        {
            if (closeButton == null)
            {
                return;
            }

            closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(closeButtonHandler);
        }

        private void BindSkinButtons()
        {
            if (skinButtons == null || skinClickHandlers == null)
            {
                return;
            }

            int count = Mathf.Min(skinButtons.Length, skinClickHandlers.Length);
            for (int i = 0; i < count; i++)
            {
                if (skinButtons[i] == null || skinClickHandlers[i] == null)
                {
                    continue;
                }

                skinButtons[i].onClick.AddListener(skinClickHandlers[i]);
            }
        }

        private void UnbindSkinButtons()
        {
            if (skinButtons == null || skinClickHandlers == null)
            {
                return;
            }

            int count = Mathf.Min(skinButtons.Length, skinClickHandlers.Length);
            for (int i = 0; i < count; i++)
            {
                if (skinButtons[i] == null || skinClickHandlers[i] == null)
                {
                    continue;
                }

                skinButtons[i].onClick.RemoveListener(skinClickHandlers[i]);
            }
        }

        private void OnCloseClicked()
        {
            closeCallback?.Invoke();
        }

        private void OnSkinClicked(int index)
        {
            if (skinButtons == null || skinButtons.Length == 0)
            {
                return;
            }

            int clampedIndex = Mathf.Clamp(index, 0, skinButtons.Length - 1);
            PlayerPrefs.SetInt(PlayerSkinKey, clampedIndex);
            PlayerPrefs.Save();
            ApplySelection(clampedIndex);
        }

        private void RefreshSelection()
        {
            if (skinButtons == null || skinButtons.Length == 0)
            {
                return;
            }

            ApplySelection(GetSavedSkinIndex());
        }

        private int GetSavedSkinIndex()
        {
            int rawValue = PlayerPrefs.GetInt(PlayerSkinKey, 0);
            int clampedValue = Mathf.Clamp(rawValue, 0, skinButtons.Length - 1);

            if (clampedValue != rawValue)
            {
                PlayerPrefs.SetInt(PlayerSkinKey, clampedValue);
                PlayerPrefs.Save();
            }

            return clampedValue;
        }

        private void ApplySelection(int selectedIndex)
        {
            for (int i = 0; i < skinButtons.Length; i++)
            {
                if (skinButtons[i] == null || skinButtons[i].image == null)
                {
                    continue;
                }

                skinButtons[i].image.color = i == selectedIndex ? selectedColor : unselectedColor;
                skinButtons[i].transform.localScale = i == selectedIndex ? Vector3.one * 1.05f : Vector3.one;
            }
        }

        private Sprite[] GetAvailableSprites()
        {
            if (skinSprites != null && skinSprites.Length > 0)
            {
                return skinSprites;
            }

            if (skinButtons == null || skinButtons.Length == 0)
            {
                return Array.Empty<Sprite>();
            }

            List<Sprite> availableSprites = new List<Sprite>();
            foreach (Button button in skinButtons)
            {
                if (button?.image?.sprite != null)
                {
                    availableSprites.Add(button.image.sprite);
                }
            }

            return availableSprites.ToArray();
        }
    }
}
