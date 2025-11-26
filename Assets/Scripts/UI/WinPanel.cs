using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.ARKANOID
{
    public class WinPanel : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreText = null;

        [SerializeField]
        private Button restartButton = null;

        private Action restartCallback = null;

        public void Show(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetRestartCallback(Action callback)
        {
            restartCallback = callback;
        }

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
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
