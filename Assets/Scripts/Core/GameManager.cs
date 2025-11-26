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

        private readonly SignalBus signalBus;
        private readonly LevelManager levelManager;

        private int lives = 0;
        private int score = 0;

        public GameManager(SignalBus signalBus, LevelManager levelManager, GameSettings gameSettings)
        {
            this.signalBus = signalBus;
            this.levelManager = levelManager;

            this.signalBus.Subscribe<BallLostSignal>(HandleBallLost);
            this.signalBus.Subscribe<BrickDestroyedSignal>(HandleBrickDestroyed);

            DefaultLives = gameSettings.defaultLives;
        }

        public void StartGame()
        {
            RestartGame(true);
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

        private void HandleBallLost()
        {
            lives--;

            signalBus.Fire(new LivesChangedSignal(lives));

            if (lives <= 0)
            {
                signalBus.Fire<GameOverSignal>();
                Time.timeScale = 0f;
            }
        }

        public void RestartGame(bool resetScore)
        {
            Time.timeScale = 1f;
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

        private void CompleteLevel()
        {
            Time.timeScale = 0f;
            signalBus.Fire<LevelCompletedSignal>();
        }
    }
}
