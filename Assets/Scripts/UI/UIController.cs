using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private HUDView hudView = null;

        [SerializeField]
        private GameOverPanel gameOverPanel = null;

        [SerializeField]
        private WinPanel winPanel = null;

        private SignalBus signalBus = null;
        private GameManager gameManager = null;
        private int lastScore = 0;

        [Inject]
        public void Construct(SignalBus signalBus, GameManager gameManager)
        {
            this.signalBus = signalBus;
            this.gameManager = gameManager;
        }

        private void OnEnable()
        {
            Subscribe();
            BindRestartButtons();
        }

        private void OnDisable()
        {
            UnbindRestartButtons();
            Unsubscribe();
        }

        private void SetScore(int value)
        {
            if (hudView != null)
            {
                hudView.SetScore(value);
            }

            lastScore = value;
        }

        private void SetLives(int value)
        {
            if (hudView != null)
            {
                hudView.SetLives(value);
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.Show();
            }
        }

        private void ShowWin(int score)
        {
            if (winPanel != null)
            {
                winPanel.Show(score);
            }
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
        }

        private void OnRestartFromWin()
        {
            HideAllPanels();
            if (gameManager != null)
            {
                gameManager.RestartGame(false);
            }
        }

        private void OnRestartFromGameOver()
        {
            HideAllPanels();
            if (gameManager != null)
            {
                gameManager.RestartGame(true);
            }
        }

        private void Subscribe()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
            signalBus.Subscribe<LivesChangedSignal>(OnLivesChanged);
            signalBus.Subscribe<GameOverSignal>(OnGameOver);
            signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
        }

        private void Unsubscribe()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);
            signalBus.Unsubscribe<LivesChangedSignal>(OnLivesChanged);
            signalBus.Unsubscribe<GameOverSignal>(OnGameOver);
            signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
        }

        private void OnScoreChanged(ScoreChangedSignal signal)
        {
            SetScore(signal.Score);
        }

        private void OnLivesChanged(LivesChangedSignal signal)
        {
            SetLives(signal.Lives);
        }

        private void OnGameOver()
        {
            ShowGameOver();
        }

        private void OnLevelCompleted()
        {
            ShowWin(lastScore);
        }

        private void OnLevelReset()
        {
            HideAllPanels();
        }

        private void BindRestartButtons()
        {
            if (winPanel != null)
            {
                winPanel.SetRestartCallback(OnRestartFromWin);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetRestartCallback(OnRestartFromGameOver);
            }
        }

        private void UnbindRestartButtons()
        {
            if (winPanel != null)
            {
                winPanel.SetRestartCallback(null);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetRestartCallback(null);
            }
        }
    }
}
