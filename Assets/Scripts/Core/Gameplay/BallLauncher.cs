using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField]
        private Ball ball = null;

        [SerializeField]
        private Transform centerSpawnPoint = null;

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private bool awaitingLaunch = false;
        private bool isMazeActive = false;

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
        }

        private void OnEnable()
        {
            SubscribeSignals();
        }

        private void OnDisable()
        {
            UnsubscribeSignals();
        }

        private void Start()
        {
            ResetBallToCenter();
        }

        private void Update()
        {
            if (!awaitingLaunch)
            {
                return;
            }

            if (isMazeActive)
            {
                return;
            }

            if (inputService != null && inputService.TryConsumeLaunchDirection(out Vector2 launchDirection))
            {
                LaunchBall(launchDirection);
            }
        }

        private void ResetBallToCenter()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = true;
            ball.ResetPosition(GetCenterSpawnPosition());
        }

        private void LaunchBall(Vector2 direction)
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = false;
            ball.Launch(direction);
        }

        private void OnBallReset()
        {
            ResetBallToCenter();
        }

        private void OnLevelReset()
        {
            ResetBallToCenter();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallResetSignal>(OnBallReset);
            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Subscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void UnsubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<BallResetSignal>(OnBallReset);
            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Unsubscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void OnMazeStarted()
        {
            isMazeActive = true;
        }

        private void OnMazeEnded()
        {
            isMazeActive = false;
        }

        private Vector2 GetCenterSpawnPosition()
        {
            if (centerSpawnPoint != null)
            {
                return centerSpawnPoint.position;
            }

            return Vector2.zero;
        }
    }
}
