using System;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.Events;

namespace MiniIT.ARKANOID
{
    public class PrivacyPolicyWindow : MonoBehaviour, IWindow
    {
        [SerializeField]
        private UIButton closeButton = null;

        private UIView view = null;

        private Action closeCallback = null;

        private void Awake()
        {
            view = GetComponent<UIView>();
            view.Hide();
        }

        public void Show()
        {
            view.Show();
        }

        public void Hide()
        {
            view.Hide();
        }

        public void SetCloseCallback(Action callback)
        {
            closeCallback = callback;
        }

        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
                closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(OnCloseClicked);
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(OnCloseClicked);
            }
        }

        private void OnCloseClicked()
        {
            if (closeCallback != null)
            {
                closeCallback.Invoke();
            }
        }
    }
}
