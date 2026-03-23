using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Ball : MonoBehaviour
    {
        private const float CollisionSkin = 0.01f;
        private const int MaxCollisionIterations = 4;

        private float launchSpeed;

        private CircleCollider2D ballCollider = null;

        private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

        private ContactFilter2D castFilter;

        private Vector2 currentDirection = Vector2.zero;

        private float currentSpeed = 0.0f;

        private bool launched = false;

        private AudioService _audioService;
        
        [Inject]
        public void Construct(GameSettings gameSettings, AudioService audioService)
        {
            launchSpeed = gameSettings.launchSpeed;
            _audioService = audioService;
        }
        private void Awake()
        {
            if (ballCollider == null)
            {
                ballCollider = GetComponent<CircleCollider2D>();
            }

            castFilter.useTriggers = true;
            castFilter.useLayerMask = false;
            castFilter.useDepth = false;
            Stop();
        }

        private void Update()
        {
            if (!launched || currentSpeed <= 0.0f || ballCollider == null)
            {
                return;
            }

            Physics2D.SyncTransforms();
            MoveBall(currentSpeed * Time.deltaTime);
        }

        public void Launch(Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            currentDirection = direction.normalized;
            currentSpeed = launchSpeed;
            launched = true;
            _audioService.PlaySound(AudioService.SoundType.LaunchSound);
        }

        public void Stop()
        {
            currentDirection = Vector2.zero;
            currentSpeed = 0.0f;
            launched = false;
        }

        public void ResetPosition(Vector2 position)
        {
            transform.position = position;
            Stop();
        }

        private void MoveBall(float distance)
        {
            float remainingDistance = distance;
            Vector2 position = transform.position;

            for (int iteration = 0; iteration < MaxCollisionIterations; iteration++)
            {
                if (!launched || remainingDistance <= 0.0f)
                {
                    break;
                }

                transform.position = position;

                RaycastHit2D hit = FindClosestHit(remainingDistance + CollisionSkin);
                if (hit.collider == null)
                {
                    position += currentDirection * remainingDistance;
                    transform.position = position;
                    return;
                }

                float moveDistance = Mathf.Max(hit.distance - CollisionSkin, 0.0f);
                if (moveDistance > 0.0f)
                {
                    position += currentDirection * moveDistance;
                    transform.position = position;
                    remainingDistance -= moveDistance;
                }

                if (TryHandleTrigger(hit))
                {
                    return;
                }

                ResolveCollision(hit);
                if (!launched)
                {
                    return;
                }

                position = (Vector2)transform.position + currentDirection * CollisionSkin;
                transform.position = position;
                remainingDistance = Mathf.Max(remainingDistance - CollisionSkin, 0.0f);
            }

            if (launched && remainingDistance > 0.0f)
            {
                transform.position = (Vector2)transform.position + currentDirection * remainingDistance;
            }
        }

        private RaycastHit2D FindClosestHit(float distance)
        {
            Vector2 origin = ballCollider.bounds.center;
            float radius = GetCastRadius();
            int hitCount = Physics2D.CircleCast(origin, radius, currentDirection, castFilter, castHits, distance);

            RaycastHit2D closestHit = default;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = castHits[i];
                Collider2D hitCollider = hit.collider;
                if (hitCollider == null)
                {
                    continue;
                }

                if (hitCollider == ballCollider || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hitCollider.isTrigger && hitCollider.GetComponentInParent<BallOutOfBounds>() == null)
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestHit = hit;
                    closestDistance = hit.distance;
                }
            }

            return closestHit;
        }

        private float GetCastRadius()
        {
            Bounds bounds = ballCollider.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        private bool TryHandleTrigger(RaycastHit2D hit)
        {
            Collider2D hitCollider = hit.collider;
            if (hitCollider == null || !hitCollider.isTrigger)
            {
                return false;
            }

            BallOutOfBounds outOfBounds = hitCollider.GetComponentInParent<BallOutOfBounds>();
            if (outOfBounds == null)
            {
                return false;
            }

            outOfBounds.HandleBallEntered(this);
            return true;
        }

        private void ResolveCollision(RaycastHit2D hit)
        {
            Collider2D hitCollider = hit.collider;
            if (hitCollider == null)
            {
                return;
            }

            Paddle paddle = hitCollider.GetComponentInParent<Paddle>();
            if (paddle != null)
            {
                BounceFromPaddle(hitCollider, paddle);
                return;
            }

            BrickBase brick = hitCollider.GetComponentInParent<BrickBase>();
            if (brick != null)
            {
                brick.HandleBallHit();
            }

            Vector2 normal = hit.normal;
            if (normal.sqrMagnitude <= Mathf.Epsilon)
            {
                normal = -currentDirection;
            }

            currentDirection = Vector2.Reflect(currentDirection, normal).normalized;
            currentSpeed = launchSpeed;
            _audioService.PlaySound(AudioService.SoundType.HitSound);
        }

        private void BounceFromPaddle(Collider2D paddleCollider, Paddle paddle)
        {
            Vector2 contactPoint = paddleCollider.ClosestPoint(transform.position);

            float halfWidth = paddleCollider.bounds.extents.x;
            if (halfWidth <= 0.0f)
            {
                halfWidth = 0.5f;
            }

            float offset = (contactPoint.x - paddle.transform.position.x) / halfWidth;
            offset = Mathf.Clamp(offset, -1.0f, 1.0f);

            currentDirection = new Vector2(offset, 1.0f).normalized;
            currentSpeed = launchSpeed;
            _audioService.PlaySound(AudioService.SoundType.LaunchSound);
        }
    }
}
