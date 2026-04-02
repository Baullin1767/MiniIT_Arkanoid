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

        [SerializeField]
        private Collider2D collisionCollider = null;

        private int health = 0;
        private bool destroyed = false;

        private SignalBus signalBus = null;
        private LevelManager levelManager = null;
        private BrickDestroyedSignal destroyedSignal;

        public Vector2Int GridPosition { get; private set; }

        public BrickType AssignedBrickType { get; private set; }

        protected LevelManager LevelManager => levelManager;

        [Inject]
        public void Construct(SignalBus signalBus, LevelManager levelManager)
        {
            this.signalBus = signalBus;
            this.levelManager = levelManager;
        }

        protected virtual void Awake()
        {
            if (collisionCollider == null)
            {
                collisionCollider = GetComponent<Collider2D>();
            }

            health = maxHealth;
        }

        protected virtual void OnEnable()
        {
            health = maxHealth;
            destroyed = false;
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

            BrickImpactContext context = BrickImpactContext.DirectHit(ball, ball.CurrentDirection);
            levelManager?.HandleBrickImpact(this, context);
        }

        protected virtual void OnDestroy()
        {
            Unregister();
        }

        public void PrepareForSpawn(BrickType brickType, Vector2Int gridPosition)
        {
            AssignedBrickType = brickType;
            GridPosition = gridPosition;
        }

        public BrickImpactResult HandleImpact(BrickImpactContext context)
        {
            if (destroyed || !gameObject.activeInHierarchy)
            {
                return BrickImpactResult.None;
            }

            bool shouldApplyDamage = true;
            BrickImpactResult impactResult = BeforeImpact(context, ref shouldApplyDamage);
            if (!shouldApplyDamage)
            {
                return impactResult;
            }

            health--;

            if (health <= 0)
            {
                DestroyBrick(context);
                return impactResult;
            }

            OnHit(health);
            return impactResult;
        }

        public Vector2 GetExitPosition(Vector2 direction, float extraDistance)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude <= Mathf.Epsilon
                ? Vector2.up
                : direction.normalized;

            Bounds bounds = collisionCollider != null
                ? collisionCollider.bounds
                : new Bounds(transform.position, Vector3.one);

            float halfExtent = Mathf.Abs(normalizedDirection.x) * bounds.extents.x +
                               Mathf.Abs(normalizedDirection.y) * bounds.extents.y;

            return (Vector2)bounds.center + normalizedDirection * (halfExtent + extraDistance);
        }

        protected virtual BrickImpactResult BeforeImpact(BrickImpactContext context, ref bool shouldApplyDamage)
        {
            return BrickImpactResult.None;
        }

        protected virtual void DestroyBrick(BrickImpactContext context)
        {
            if (destroyed)
            {
                return;
            }

            destroyed = true;
            Unregister();
            OnDestroyed(context);

            if (signalBus != null)
            {
                destroyedSignal.Brick = this;
                destroyedSignal.Reward = ResolveScoreReward(context);
                signalBus.Fire(destroyedSignal);
            }

            gameObject.SetActive(false);
        }

        protected virtual void OnHit(int remainingHealth)
        {
        }

        protected virtual void OnDestroyed(BrickImpactContext context)
        {
        }

        protected virtual int ResolveScoreReward(BrickImpactContext context)
        {
            return scoreReward;
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
