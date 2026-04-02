using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Coordinates overall game state such as score, lives, and victory/defeat conditions.
    /// </summary>
    public class GameManager
    {
        private int DefaultLives = 3;
        private const int MazeLifeReward = 1;

        private readonly SignalBus signalBus;
        private readonly LevelManager levelManager;

        private int lives = 0;
        private int score = 0;
        private bool isMazeRescueActive = false;

        public GameManager(SignalBus signalBus, LevelManager levelManager, GameSettings gameSettings)
        {
            this.signalBus = signalBus;
            this.levelManager = levelManager;

            this.signalBus.Subscribe<BrickDestroyedSignal>(HandleBrickDestroyed);
            this.signalBus.Subscribe<MazeCompletedSignal>(HandleMazeCompleted);
            this.signalBus.Subscribe<MazeFailedSignal>(HandleMazeFailed);

            DefaultLives = gameSettings.defaultLives;
        }

        public void StartGame()
        {
            RestartGame(true);
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        private void HandleBrickDestroyed(BrickDestroyedSignal signal)
        {
            score += signal.Reward;

            signalBus.Fire(new ScoreChangedSignal(score));

            if (levelManager.IsLevelComplete())
            {
                CompleteLevel();
            }
        }

        private void HandleMazeCompleted()
        {
            if (!isMazeRescueActive)
            {
                return;
            }

            isMazeRescueActive = false;
            lives += MazeLifeReward;

            signalBus.Fire(new LivesChangedSignal(lives));
        }

        private void HandleMazeFailed()
        {
            if (!isMazeRescueActive)
            {
                return;
            }

            isMazeRescueActive = false;

            if (lives > 0)
            {
                return;
            }

            signalBus.Fire<GameOverSignal>();
            PauseGame();
        }

        public void RestartGame(bool resetScore)
        {
            ResumeGame();
            isMazeRescueActive = false;
            if (resetScore)
            {
                score = 0;
            }

            lives = DefaultLives;

            signalBus.Fire(new ScoreChangedSignal(score));
            signalBus.Fire(new LivesChangedSignal(lives));

            levelManager.ResetLevel();
            signalBus.Fire<LevelResetSignal>();
        }

        public void RestartCurrentRound()
        {
            ResumeGame();
            isMazeRescueActive = false;
            lives = DefaultLives;

            signalBus.Fire(new ScoreChangedSignal(score));
            signalBus.Fire(new LivesChangedSignal(lives));

            levelManager.ResetLevel(true);
            signalBus.Fire<LevelResetSignal>();
        }

        private void CompleteLevel()
        {
            isMazeRescueActive = false;
            PauseGame();
            signalBus.Fire<LevelCompletedSignal>();
        }
    }
}
