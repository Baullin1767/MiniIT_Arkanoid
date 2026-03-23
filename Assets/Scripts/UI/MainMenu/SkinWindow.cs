using System;
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
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color unselectedColor = Color.gray;

        private UnityAction[] skinClickHandlers = Array.Empty<UnityAction>();

        private UIView view = null;
        private Action closeCallback = null;
        private UnityAction closeButtonHandler = null;

        private void Awake()
        {
            view = GetComponent<UIView>();
            closeButtonHandler = OnCloseClicked;

            if (view != null)
            {
                view.Hide();
            }

            if (skinButtons == null || skinButtons.Length == 0)
            {
                return;
            }

            Array.Resize(ref skinClickHandlers, skinButtons.Length);
            for (int i = 0; i < skinButtons.Length; i++)
            {
                int index = i;
                skinClickHandlers[i] = () => OnSkinClicked(index);
            }

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
            }
        }
    }
}
