using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        private const float MinimumLaunchUpwardComponent = 0.35f;

        [SerializeField]
        private Ball ball = null;

        [SerializeField]
        private Vector2 attachOffset = Vector2.zero;

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private BallCoordinator ballCoordinator = null;
        private bool awaitingLaunch = false;
        private bool isMazeActive = false;

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus, BallCoordinator ballCoordinator)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
            this.ballCoordinator = ballCoordinator;
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
            ballCoordinator?.SetPrimaryBall(ball);
            ResetBallToLaunchPoint();
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

            if (inputService != null && inputService.IsLaunchRequested())
            {
                LaunchBall();
            }
        }

        private void ResetBallToLaunchPoint()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = true;
            ball.Stop();
            ball.ResetPosition(GetLaunchPosition());
        }

        private void LaunchBall()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = false;

            Vector2 launchDirection = ResolveLaunchDirection();
            ball.Launch(launchDirection);
        }

        private void OnBallLost()
        {
            ResetBallToLaunchPoint();
        }

        private void OnLevelReset()
        {
            ResetBallToLaunchPoint();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallLostSignal>(OnBallLost);
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

            signalBus.Unsubscribe<BallLostSignal>(OnBallLost);
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

        private Vector2 GetLaunchPosition()
        {
            return (Vector2)transform.position + attachOffset;
        }

        private Vector2 ResolveLaunchDirection()
        {
            Vector2 inputDirection = inputService != null
                ? inputService.GetMoveInput()
                : Vector2.zero;

            if (inputDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector2.up;
            }

            Vector2 launchDirection = inputDirection.normalized;
            if (launchDirection.y < MinimumLaunchUpwardComponent)
            {
                launchDirection.y = MinimumLaunchUpwardComponent;
            }

            return launchDirection.normalized;
        }
    }
}
