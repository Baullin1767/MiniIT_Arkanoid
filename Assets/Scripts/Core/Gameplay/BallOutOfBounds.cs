using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallOutOfBounds : MonoBehaviour
    {
        private BallCoordinator ballCoordinator = null;

        [Inject]
        public void Construct(BallCoordinator ballCoordinator)
        {
            this.ballCoordinator = ballCoordinator;
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

            ballCoordinator?.HandleBallOutOfBounds(ball);
        }
    }
}
