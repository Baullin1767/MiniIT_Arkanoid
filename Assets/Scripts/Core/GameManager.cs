using System;
using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class GameManager : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly GameSettings gameSettings;
        private int lives;
        private int score;

        public GameManager(SignalBus signalBus, GameSettings gameSettings)
        {
            this.signalBus = signalBus;
            this.gameSettings = gameSettings;
        }

        public void Initialize()
        {
            signalBus.Subscribe<BallLostSignal>(OnBallLost);
            signalBus.Subscribe<BallMergedSignal>(OnBallMerged);
            ResetLives();
            ResetScore();
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<BallLostSignal>(OnBallLost);
            signalBus.Unsubscribe<BallMergedSignal>(OnBallMerged);
        }

        public void StartGame()
        {
            RestartGame(false);
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        public void RestartGame(bool resetScore)
        {
            ResumeGame();
            ResetLives();
            ResetScore();
            signalBus.Fire<LevelResetSignal>();
        }

        public void RestartCurrentRound()
        {
            ResumeGame();
            signalBus.Fire<LevelResetSignal>();
        }

        private void OnBallLost()
        {
            lives--;
            signalBus.Fire(new LivesChangedSignal(lives));

            if (lives <= 0)
            {
                signalBus.Fire(new GameOverSignal(score));
            }
        }

        private void OnBallMerged(BallMergedSignal signal)
        {
            score += signal.Tier * 100;
            signalBus.Fire(new ScoreChangedSignal(score));
        }

        private void ResetLives()
        {
            lives = gameSettings.defaultLives;
            signalBus.Fire(new LivesChangedSignal(lives));
        }

        private void ResetScore()
        {
            score = 0;
            signalBus.Fire(new ScoreChangedSignal(score));
        }
    }
}
