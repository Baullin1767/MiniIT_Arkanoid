using System;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.Events;

namespace MiniIT.ARKANOID
{
    public class PausePanel : MonoBehaviour
    {
        [SerializeField]
        private UIButton resumeButton = null;

        [SerializeField]
        private UIButton restartButton = null;

        [SerializeField]
        private UIButton menuButton = null;

        private UIView view = null;

        private Action resumeCallback = null;
        private Action restartCallback = null;
        private Action menuCallback = null;

        private void Awake()
        {
            view = GetComponent<UIView>();
            view.Hide();
        }

        private void OnEnable()
        {
            RegisterButton(resumeButton, OnResumeClicked);
            RegisterButton(restartButton, OnRestartClicked);
            RegisterButton(menuButton, OnMenuClicked);
        }

        private void OnDisable()
        {
            UnregisterButton(resumeButton, OnResumeClicked);
            UnregisterButton(restartButton, OnRestartClicked);
            UnregisterButton(menuButton, OnMenuClicked);
        }

        public void Show()
        {
            view.Show();
        }

        public void Hide()
        {
            view.Hide();
        }

        public void SetResumeCallback(Action callback)
        {
            resumeCallback = callback;
        }

        public void SetRestartCallback(Action callback)
        {
            restartCallback = callback;
        }

        public void SetMenuCallback(Action callback)
        {
            menuCallback = callback;
        }

        private void RegisterButton(UIButton button, UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
            button.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(handler);
        }

        private void UnregisterButton(UIButton button, UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(handler);
        }

        private void OnResumeClicked()
        {
            resumeCallback?.Invoke();
        }

        private void OnRestartClicked()
        {
            restartCallback?.Invoke();
        }

        private void OnMenuClicked()
        {
            menuCallback?.Invoke();
        }
    }
}
