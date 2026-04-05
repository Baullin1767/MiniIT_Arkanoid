using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class GameManager
    {
        private readonly SignalBus signalBus;

        public GameManager(SignalBus signalBus)
        {
            this.signalBus = signalBus;
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
            signalBus.Fire<LevelResetSignal>();
        }

        public void RestartCurrentRound()
        {
            ResumeGame();
            signalBus.Fire<LevelResetSignal>();
        }
    }
}
