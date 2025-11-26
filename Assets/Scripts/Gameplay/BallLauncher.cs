using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField]
        private Ball ball = null;

        [SerializeField]
        private Vector2 attachOffset = new Vector2(0.0f, 0.6f);

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private bool awaitingLaunch = false;

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
            AttachBallToPaddle();
        }

        private void Update()
        {
            if (!awaitingLaunch)
            {
                return;
            }

            FollowPaddle();

            if (inputService != null && inputService.IsLaunchRequested())
            {
                LaunchBall();
            }
        }

        private void AttachBallToPaddle()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = true;
            ball.Stop();
            FollowPaddle();
        }

        private void FollowPaddle()
        {
            if (ball == null)
            {
                return;
            }

            Vector2 attachPosition = transform.position;
            attachPosition += attachOffset;

            ball.ResetPosition(attachPosition);
        }

        private void LaunchBall()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = false;

            Vector2 launchDirection = new Vector2(0.0f, 1.0f);
            ball.Launch(launchDirection);
        }

        private void OnBallLost()
        {
            AttachBallToPaddle();
        }

        private void OnLevelReset()
        {
            AttachBallToPaddle();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallLostSignal>(OnBallLost);
            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
        }

        private void UnsubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<BallLostSignal>(OnBallLost);
            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
        }
    }
}
