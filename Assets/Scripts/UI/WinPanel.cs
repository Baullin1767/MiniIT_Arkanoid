using System;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MiniIT.ARKANOID
{
    public class WinPanel : MonoBehaviour
    {
        private const string DownloadButtonLabel = "Download the game";

        [SerializeField]
        private TMP_Text scoreText = null;

        [SerializeField]
        private UIButton restartButton = null;
        [SerializeField]
        private UIButton nextButton = null;
        
        private UIView view = null;

        private Action downloadCallback = null;

        private void Awake()
        {
            view =  GetComponent<UIView>();
            ConfigureForDownloadAction();
            view.Hide();
        }

        public void Show(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }

            ConfigureForDownloadAction();
            view.Show();
        }

        public void Hide()
        {
            view.Hide();
        }

        public void SetDownloadCallback(Action callback)
        {
            downloadCallback = callback;
        }

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event ??= new UnityEvent();
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(OnDownloadClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.RemoveListener(OnDownloadClicked);
            }
        }

        private void OnDownloadClicked()
        {
            downloadCallback?.Invoke();
        }

        private void ConfigureForDownloadAction()
        {
            if (restartButton != null)
            {
                // TMP_Text label = restartButton.GetComponentInChildren<TMP_Text>(true);
                // if (label != null)
                // {
                //     label.gameObject.SetActive(true);
                //     label.text = DownloadButtonLabel;
                // }

                restartButton.gameObject.SetActive(true);
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }
        }
    }
}
