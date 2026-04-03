using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallOutOfBounds : MonoBehaviour
    {
        private SignalBus signalBus = null;
        private LevelManager levelManager = null;

        [Inject]
        public void Construct(SignalBus signalBus, LevelManager levelManager)
        {
            this.signalBus = signalBus;
            this.levelManager = levelManager;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Ball ball = other.GetComponentInParent<Ball>();
            HandleBallEntered(ball);
        }

        public void HandleBallEntered(Ball ball)
        {
            if (ball == null)
            {
                return;
            }

            ball.Stop();

            if (signalBus != null)
            {
                signalBus.Fire<BallResetSignal>();
            }

            levelManager?.HandleBallMiss();
        }
    }
}
