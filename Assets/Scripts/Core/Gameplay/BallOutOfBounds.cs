using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallOutOfBounds : MonoBehaviour
    {
        private enum BorderAction
        {
            ResetBall = 0,
            SpawnAdditionalBall = 1
        }

        [SerializeField]
        private BorderAction action = BorderAction.ResetBall;

        private SignalBus signalBus = null;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Ball ball = other.GetComponentInParent<Ball>();
            HandleBallEntered(ball);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Ball ball = collision.collider.GetComponentInParent<Ball>();
            HandleBallEntered(ball);
        }

        public void HandleBallEntered(Ball ball)
        {
            if (ball == null)
            {
                return;
            }

            if (action == BorderAction.SpawnAdditionalBall)
            {
                if (ball.TryMarkReachedTop())
                {
                    signalBus?.Fire<BallReachedTopSignal>();
                }

                return;
            }

            ball.Stop();
            signalBus?.Fire<BallResetSignal>();
        }
    }
}
