using System.Collections;
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
        private const float TeleportDelaySeconds = 1.0f;

        private float launchSpeed;

        private CircleCollider2D ballCollider = null;
        private Renderer[] ballRenderers = null;

        private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

        private ContactFilter2D castFilter;

        private Vector2 currentDirection = Vector2.zero;

        private float currentSpeed = 0.0f;

        private bool launched = false;

        private AudioService _audioService;
        private LevelManager levelManager = null;
        private BallCoordinator ballCoordinator = null;
        private TeleportBrick ignoredTeleportBrick = null;
        private Coroutine teleportRoutine = null;
        private bool isTeleporting = false;
        
        [Inject]
        public void Construct(GameSettings gameSettings, AudioService audioService, LevelManager levelManager, BallCoordinator ballCoordinator)
        {
            launchSpeed = gameSettings.launchSpeed;
            _audioService = audioService;
            this.levelManager = levelManager;
            this.ballCoordinator = ballCoordinator;
        }
        private void Awake()
        {
            if (ballCollider == null)
            {
                ballCollider = GetComponent<CircleCollider2D>();
            }

            ballRenderers = GetComponentsInChildren<Renderer>(true);
            castFilter.useTriggers = true;
            castFilter.useLayerMask = false;
            castFilter.useDepth = false;
            Stop();
        }

       
        private void Update()
        {
            if (!launched || currentSpeed <= 0.0f || ballCollider == null || isTeleporting)
            {
                return;
            }

            Physics2D.SyncTransforms();
            MoveBall(currentSpeed * Time.deltaTime);
        }

        public Vector2 CurrentDirection => currentDirection;

        public float CurrentSpeed => currentSpeed;

        public void Launch(Vector2 direction)
        {
            Launch(direction, launchSpeed);
        }

        public void Launch(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            CancelPendingTeleport();
            currentDirection = direction.normalized;
            currentSpeed = speed > 0.0f ? speed : launchSpeed;
            launched = true;
            ignoredTeleportBrick = null;
            ballCoordinator?.SetBallInPlay(this, true);
            _audioService.PlaySound(AudioService.SoundType.LaunchSound);
        }

        public void Stop()
        {
            CancelPendingTeleport();
            currentDirection = Vector2.zero;
            currentSpeed = 0.0f;
            launched = false;
            ignoredTeleportBrick = null;
            ballCoordinator?.SetBallInPlay(this, false);
        }

        public void ResetPosition(Vector2 position)
        {
            transform.position = position;
            Stop();
        }

        public void TeleportTo(TeleportBrick destination)
        {
            if (destination == null)
            {
                return;
            }

            Vector2 travelDirection = currentDirection.sqrMagnitude <= Mathf.Epsilon
                ? Vector2.up
                : currentDirection.normalized;
            Vector2 exitPosition = destination.GetExitPosition(travelDirection, GetCastRadius() + CollisionSkin);

            CancelPendingTeleport();
            teleportRoutine = StartCoroutine(TeleportAfterDelay(destination, exitPosition));
        }

        private IEnumerator TeleportAfterDelay(TeleportBrick destination, Vector2 exitPosition)
        {
            isTeleporting = true;
            SetBallVisible(false);

            if (ballCollider != null)
            {
                ballCollider.enabled = false;
            }

            yield return new WaitForSeconds(TeleportDelaySeconds);

            transform.position = exitPosition;
            ignoredTeleportBrick = destination;

            if (ballCollider != null)
            {
                ballCollider.enabled = true;
            }

            SetBallVisible(true);
            Physics2D.SyncTransforms();
            isTeleporting = false;
            teleportRoutine = null;
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

                TeleportBrick teleportBrick = hitCollider.GetComponentInParent<TeleportBrick>();
                if (ignoredTeleportBrick != null && teleportBrick == ignoredTeleportBrick)
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
                ignoredTeleportBrick = null;
                BounceFromPaddle(hitCollider, paddle);
                return;
            }

            BrickBase brick = hitCollider.GetComponentInParent<BrickBase>();
            if (brick != null)
            {
                BrickImpactResult impactResult = levelManager != null
                    ? levelManager.HandleBrickImpact(brick, BrickImpactContext.DirectHit(this, currentDirection))
                    : brick.HandleImpact(BrickImpactContext.DirectHit(this, currentDirection));

                if (impactResult.ConsumeCollision)
                {
                    return;
                }
            }

            ignoredTeleportBrick = null;
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

        private void CancelPendingTeleport()
        {
            if (teleportRoutine != null)
            {
                StopCoroutine(teleportRoutine);
                teleportRoutine = null;
            }

            isTeleporting = false;

            if (ballCollider != null)
            {
                ballCollider.enabled = true;
            }

            SetBallVisible(true);
        }

        private void SetBallVisible(bool isVisible)
        {
            if (ballRenderers == null)
            {
                return;
            }

            for (int i = 0; i < ballRenderers.Length; i++)
            {
                Renderer ballRenderer = ballRenderers[i];
                if (ballRenderer == null)
                {
                    continue;
                }

                ballRenderer.enabled = isVisible;
            }
        }
    }
}
