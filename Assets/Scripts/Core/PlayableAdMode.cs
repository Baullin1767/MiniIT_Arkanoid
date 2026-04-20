using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class PlayableAdMode : IInitializable, ITickable, System.IDisposable
    {
        private const float DefaultDurationSeconds = 20.0f;

        private readonly SignalBus signalBus = null;
        private readonly GameSettings gameSettings = null;

        private float elapsedSeconds = 0.0f;
        private bool isSessionRunning = false;

        public PlayableAdMode(SignalBus signalBus, GameSettings gameSettings)
        {
            this.signalBus = signalBus;
            this.gameSettings = gameSettings;
        }

        public bool IsEnabled => gameSettings != null && gameSettings.enablePlayableAdMode;

        private float SessionDurationSeconds =>
            gameSettings != null && gameSettings.playableAdDurationSeconds > 0.0f
                ? gameSettings.playableAdDurationSeconds
                : DefaultDurationSeconds;

        public void Initialize()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Subscribe<LevelCompletedSignal>(OnSessionFinished);
            signalBus.Subscribe<GameOverSignal>(OnSessionFinished);
        }

        public void Dispose()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Unsubscribe<LevelCompletedSignal>(OnSessionFinished);
            signalBus.Unsubscribe<GameOverSignal>(OnSessionFinished);
        }

        public void Tick()
        {
            if (!IsEnabled || !isSessionRunning)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds < SessionDurationSeconds)
            {
                return;
            }

            isSessionRunning = false;
            signalBus.Fire<PlayableAdTimeoutSignal>();
        }

        private void OnLevelReset()
        {
            if (!IsEnabled)
            {
                isSessionRunning = false;
                return;
            }

            elapsedSeconds = 0.0f;
            isSessionRunning = true;
        }

        private void OnSessionFinished()
        {
            isSessionRunning = false;
        }
    }
}
