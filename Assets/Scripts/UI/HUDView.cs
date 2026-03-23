using System;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiniIT.ARKANOID
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private UIButton pauseButton;
        
        [SerializeField]
        private TMP_Text scoreText = null;

        [SerializeField]
        private TMP_Text livesText = null;

        private Action pauseCallback = null;

        private void OnEnable()
        {
            RegisterPauseButton();
        }

        private void OnDisable()
        {
            UnregisterPauseButton();
        }

        private void ShowPauseWindow()
        {
            pauseCallback?.Invoke();
        }

        public void SetPauseCallback(Action callback)
        {
            pauseCallback = callback;
        }

        public void SetScore(int value)
        {
            if (scoreText == null)
            {
                return;
            }
            scoreText.text = "score: " + value;
        }

        public void SetLives(int value)
        {
            if (livesText == null)
            {
                return;
            }

            livesText.text = "lives: " + value;
        }

        private void RegisterPauseButton()
        {
            if (pauseButton == null)
            {
                return;
            }

            pauseButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
            pauseButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(ShowPauseWindow);
        }

        private void UnregisterPauseButton()
        {
            if (pauseButton == null)
            {
                return;
            }

            pauseButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(ShowPauseWindow);
        }
    }
}
