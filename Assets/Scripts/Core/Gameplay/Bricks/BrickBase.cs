using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public abstract class BrickBase : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth = 1;

        [SerializeField]
        private int scoreReward = 10;

        private int health = 0;

        private SignalBus signalBus = null;
        private LevelManager levelManager = null;
        private BrickDestroyedSignal destroyedSignal;

        [Inject]
        public void Construct(SignalBus signalBus, LevelManager levelManager)
        {
            this.signalBus = signalBus;
            this.levelManager = levelManager;
        }

        protected virtual void Awake()
        {
            health = maxHealth;
        }

        protected virtual void OnEnable()
        {
            health = maxHealth;
            Register();
        }

        protected virtual void OnDisable()
        {
            Unregister();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Ball ball = collision.collider.GetComponentInParent<Ball>();
            if (ball == null)
            {
                return;
            }

            Hit();
        }

        protected virtual void OnDestroy()
        {
            Unregister();
        }

        private void Hit()
        {
            health--;

            if (health <= 0)
            {
                DestroyBrick();
                return;
            }

            OnHit(health);
        }

        protected virtual void DestroyBrick()
        {
            if (signalBus != null)
            {
                destroyedSignal.Brick = this;
                destroyedSignal.Reward = scoreReward;
                signalBus.Fire(destroyedSignal);
            }

            Unregister();
            gameObject.SetActive(false);
        }

        protected virtual void OnHit(int remainingHealth)
        {
        }

        private void Register()
        {
            if (levelManager != null)
            {
                levelManager.RegisterBrick(this);
            }
        }

        private void Unregister()
        {
            if (levelManager != null)
            {
                levelManager.UnregisterBrick(this);
            }
        }
    }
}
