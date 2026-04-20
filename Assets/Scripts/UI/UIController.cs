using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class UIController : MonoBehaviour
    {
        private const int WinScoreThreshold = 6000;
        private const string DefaultDownloadGameUrl = "https://www.google.com";

        [SerializeField]
        private HUDView hudView = null;

        [SerializeField]
        private GameOverPanel gameOverPanel = null;

        [SerializeField]
        private WinPanel winPanel = null;

        [SerializeField]
        private PausePanel pausePanel = null;

        private SignalBus signalBus = null;
        private GameManager gameManager = null;
        private GameSettings gameSettings = null;
        private MazeRescuePanel mazeRescuePanel = null;
        private bool isMazeActive = false;
        private bool isSessionFinished = false;
        private int currentScore = 0;
        private const string MainMenuSceneName = "MainMenu";

        [Inject]
        public void Construct(
            SignalBus signalBus,
            GameManager gameManager,
            GameSettings gameSettings,
            [InjectOptional] MazeRescuePanel mazeRescuePanel)
        {
            this.signalBus = signalBus;
            this.gameManager = gameManager;
            this.gameSettings = gameSettings;
            this.mazeRescuePanel = mazeRescuePanel;
        }

        private void OnEnable()
        {
            Subscribe();
            BindRestartButtons();
            BindPauseButtons();
        }

        private void OnDisable()
        {
            UnbindPauseButtons();
            UnbindRestartButtons();
            Unsubscribe();
        }

        private void HideAllPanels()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.Hide();
            }

            if (winPanel != null)
            {
                winPanel.Hide();
            }

            if (pausePanel != null)
            {
                pausePanel.Hide();
            }

            HideMazePanel();
        }

        private void OnRestartFromGameOver()
        {
            HideAllPanels();
            gameManager?.RestartGame(true);
        }

        private void OnPauseRequested()
        {
            if (IsPlayableAdModeEnabled || gameManager == null || pausePanel == null)
            {
                return;
            }

            gameManager.PauseGame();
            pausePanel.Show();
        }

        private void OnResumeFromPause()
        {
            if (pausePanel != null)
            {
                pausePanel.Hide();
            }

            gameManager?.ResumeGame();
        }

        private void OnRestartFromPause()
        {
            HideAllPanels();
            gameManager?.RestartCurrentRound();
        }

        private void OnMenuFromPause()
        {
            HideAllPanels();
            gameManager?.ResumeGame();
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void Subscribe()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Subscribe<LivesChangedSignal>(OnLivesChanged);
            signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
            signalBus.Subscribe<GameOverSignal>(OnGameOver);
            signalBus.Subscribe<PlayableAdTimeoutSignal>(OnPlayableAdTimeout);
            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Subscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void Unsubscribe()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Unsubscribe<LivesChangedSignal>(OnLivesChanged);
            signalBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);
            signalBus.Unsubscribe<GameOverSignal>(OnGameOver);
            signalBus.Unsubscribe<PlayableAdTimeoutSignal>(OnPlayableAdTimeout);
            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Unsubscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void OnLevelReset()
        {
            isMazeActive = false;
            isSessionFinished = false;
            currentScore = 0;
            HideAllPanels();
        }

        private void OnLivesChanged(LivesChangedSignal signal)
        {
            if (hudView != null)
            {
                hudView.SetLives(signal.Lives);
            }
        }

        private void OnScoreChanged(ScoreChangedSignal signal)
        {
            currentScore = signal.Score;

            if (hudView != null)
            {
                hudView.SetScore(signal.Score);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetScore(signal.Score);
            }

            if (!isSessionFinished && signal.Score >= WinScoreThreshold)
            {
                ShowWinPopup(signal.Score);
            }
        }

        private void OnGameOver()
        {
            if (isSessionFinished)
            {
                return;
            }

            if (IsPlayableAdModeEnabled)
            {
                ShowPlayableAdCta(currentScore);
                return;
            }

            isSessionFinished = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.Show();
            }
        }

        private void OnMazeStarted()
        {
            isMazeActive = true;
        }

        private void OnMazeEnded()
        {
            isMazeActive = false;
        }

        private void BindRestartButtons()
        {
            if (winPanel != null)
            {
                winPanel.SetDownloadCallback(OnDownloadFromWin);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetRestartCallback(OnRestartFromGameOver);
            }
        }

        private void BindPauseButtons()
        {
            if (hudView != null)
            {
                hudView.SetPauseCallback(OnPauseRequested);
                hudView.SetPauseButtonVisible(!IsPlayableAdModeEnabled);
            }

            if (pausePanel != null)
            {
                pausePanel.SetResumeCallback(OnResumeFromPause);
                pausePanel.SetRestartCallback(OnRestartFromPause);
                pausePanel.SetMenuCallback(OnMenuFromPause);
            }
        }

        private void UnbindRestartButtons()
        {
            if (winPanel != null)
            {
                winPanel.SetDownloadCallback(null);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetRestartCallback(null);
            }
        }

        private void UnbindPauseButtons()
        {
            if (hudView != null)
            {
                hudView.SetPauseCallback(null);
            }

            if (pausePanel != null)
            {
                pausePanel.SetResumeCallback(null);
                pausePanel.SetRestartCallback(null);
                pausePanel.SetMenuCallback(null);
            }
        }

        private void HideMazePanel()
        {
            mazeRescuePanel?.HideImmediate();
        }

        private void ShowWinPopup(int score)
        {
            isSessionFinished = true;
            HideAllPanels();
            gameManager?.PauseGame();
            signalBus?.Fire<LevelCompletedSignal>();
            winPanel?.Show(score);
        }

        private void OnDownloadFromWin()
        {
            Application.OpenURL(ResolveDownloadUrl());
        }

        private void OnPlayableAdTimeout()
        {
            if (isSessionFinished)
            {
                return;
            }

            ShowPlayableAdCta(currentScore);
        }

        private void ShowPlayableAdCta(int score)
        {
            isSessionFinished = true;
            HideAllPanels();
            gameManager?.PauseGame();
            winPanel?.Show(score);
        }

        private string ResolveDownloadUrl()
        {
            if (gameSettings != null && !string.IsNullOrWhiteSpace(gameSettings.playableAdCtaUrl))
            {
                return gameSettings.playableAdCtaUrl;
            }

            return DefaultDownloadGameUrl;
        }

        private bool IsPlayableAdModeEnabled =>
            gameSettings != null && gameSettings.enablePlayableAdMode;
    }
}
