using System;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.Events;

namespace MiniIT.ARKANOID
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField]
        private UIButton restartButton = null;

        private UIView view = null;
        
        private Action restartCallback = null;

        private void Awake()
        {
            view =  GetComponent<UIView>();
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

        public void SetRestartCallback(Action callback)
        {
            restartCallback = callback;
        }

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(OnRestartClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(OnRestartClicked);
            }
        }

        private void OnRestartClicked()
        {
            if (restartCallback != null)
            {
                restartCallback.Invoke();
            }
        }
    }
}
