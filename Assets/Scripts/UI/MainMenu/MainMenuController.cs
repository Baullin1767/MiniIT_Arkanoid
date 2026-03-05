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
        private UIButton startButton = null;

        [SerializeField]
        private UIButton leaderBoardButton = null;

        [SerializeField]
        private UIButton aboutButton = null;

        [SerializeField]
        private UIButton privacyPolicyButton = null;

        [SerializeField]
        private WindowsManager windowsManager = null;

        private const string GameSceneName = "Game";

        private void OnEnable()
        {
            RegisterButton(startButton, OnStartClicked);
            RegisterButton(leaderBoardButton, OnLeaderBoardClicked);
            RegisterButton(aboutButton, OnAboutClicked);
            RegisterButton(privacyPolicyButton, OnPrivacyPolicyClicked);
        }

        private void OnDisable()
        {
            UnregisterButton(startButton, OnStartClicked);
            UnregisterButton(leaderBoardButton, OnLeaderBoardClicked);
            UnregisterButton(aboutButton, OnAboutClicked);
            UnregisterButton(privacyPolicyButton, OnPrivacyPolicyClicked);
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

        private void OnStartClicked()
        {
            SceneManager.LoadScene(GameSceneName);
        }

        private void OnLeaderBoardClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowLeaderBoard();
            }
        }

        private void OnAboutClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowAbout();
            }
        }

        private void OnPrivacyPolicyClicked()
        {
            if (windowsManager != null)
            {
                windowsManager.ShowPrivacyPolicy();
            }
        }
    }
}
