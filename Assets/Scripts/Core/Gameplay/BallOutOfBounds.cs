using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallOutOfBounds : MonoBehaviour
    {
        private SignalBus signalBus = null;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Ball ball = other.GetComponentInParent<Ball>();
            if (ball == null)
            {
                return;
            }

            ball.Stop();

            if (signalBus != null)
            {
                signalBus.Fire<BallLostSignal>();
            }
        }
    }
}
