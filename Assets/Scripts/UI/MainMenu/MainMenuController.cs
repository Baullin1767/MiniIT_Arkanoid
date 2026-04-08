using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MiniIT.ARKANOID
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private UIButton playButton = null;

        [SerializeField]
        private UIButton aboutButton = null;

        [SerializeField]
        private UIButton shopButton = null;
        private UIButton shopButtonClose = null;

        [SerializeField]
        private WindowsManager windowsManager = null;

        private const string GameSceneName = "Game";

        private void Awake()
        {
            // if (shopButton != null)
            // {
            //     shopButton.gameObject.SetActive(false);
            // }
        }

        private void OnEnable()
        {
            RegisterButton(playButton, OnPlayClicked);
            RegisterButton(aboutButton, OnAboutClicked);
            RegisterButton(shopButton, OnShopClicked);
            RegisterButton(shopButtonClose, OnShopClickedClose);
        }

        private void OnDisable()
        {
            UnregisterButton(playButton, OnPlayClicked);
            UnregisterButton(aboutButton, OnAboutClicked);
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

        private void OnAboutClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowAbout();
            }
        }
        private void OnShopClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowSettings();
            }
        }
        private void OnShopClickedClose()
        {
            if (windowsManager != null)
            {
                windowsManager.HideAll();
            }
        }
    }
}
