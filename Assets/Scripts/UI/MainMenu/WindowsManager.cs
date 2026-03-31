using System;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private LeaderBoardWindow leaderBoardWindow = null;

        [SerializeField]
        private AboutWindow aboutWindow = null;

        [SerializeField]
        private PrivacyPolicyWindow privacyPolicyWindow = null;

        [SerializeField]
        private SkinWindow skinWindow = null;

        [SerializeField]
        private SettingsWindow settingsWindow = null;

        private IWindow currentWindow = null;

        private void Awake()
        {
            if (leaderBoardWindow != null)
            {
                leaderBoardWindow.SetCloseCallback(HideAll);
            }

            if (aboutWindow != null)
            {
                aboutWindow.SetCloseCallback(HideAll);
            }

            if (privacyPolicyWindow != null)
            {
                privacyPolicyWindow.SetCloseCallback(HideAll);
            }

            if (skinWindow != null)
            {
                skinWindow.SetCloseCallback(HideAll);
            }

            if (settingsWindow != null)
            {
                settingsWindow.Configure(ShowLeaderBoard, ShowAbout, ShowPrivacyPolicy, HideAll);
            }
        }

        public void ShowLeaderBoard()
        {
            ShowWindow(leaderBoardWindow);
        }

        public void ShowAbout()
        {
            ShowWindow(aboutWindow);
        }

        public void ShowPrivacyPolicy()
        {
            ShowWindow(privacyPolicyWindow);
        }

        public void ShowSkin()
        {
            ShowWindow(skinWindow);
        }

        public void ShowSettings()
        {
            ShowWindow(settingsWindow);
        }

        public void HideAll()
        {
            if (currentWindow != null)
            {
                currentWindow.Hide();
                currentWindow = null;
            }
        }

        private void ShowWindow(IWindow target)
        {
            if (target == null)
            {
                return;
            }

            if (currentWindow == target)
            {
                HideAll();
                return;
            }

            HideAll();
            currentWindow = target;
            currentWindow.Show();
        }
    }
}
