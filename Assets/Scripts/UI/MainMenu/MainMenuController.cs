using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MiniIT.ARKANOID
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private UIButton playButton = null;

        [SerializeField]
        private UIButton settingsButton = null;

        [SerializeField]
        private UIButton shopButton = null;

        [SerializeField]
        private WindowsManager windowsManager = null;

        private const string GameSceneName = "Game";

        private void Awake()
        {
            if (shopButton != null)
            {
                shopButton.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            RegisterButton(playButton, OnPlayClicked);
            RegisterButton(settingsButton, OnSettingsClicked);
        }

        private void OnDisable()
        {
            UnregisterButton(playButton, OnPlayClicked);
            UnregisterButton(settingsButton, OnSettingsClicked);
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

        private void OnPlayClicked()
        {
            SceneManager.LoadScene(GameSceneName);
        }

        private void OnSettingsClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowSettings();
            }
        }
    }
}
