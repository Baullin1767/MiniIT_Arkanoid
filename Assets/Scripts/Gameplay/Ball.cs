using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Ball : MonoBehaviour
    {
        private float launchSpeed;

        private float wallBounceMultiplier;

        private Rigidbody2D body = null;

        private bool launched = false;
        
        [Inject]
        public void Construct(GameSettings gameSettings)
        {
            launchSpeed = gameSettings.launchSpeed;
            wallBounceMultiplier = gameSettings.wallBounceMultiplier;
        }
        private void Awake()
        {
            
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            Stop();
        }

        public void Launch(Vector2 direction)
        {
            if (body == null)
            {
                return;
            }

            Vector2 normalizedDirection = direction.normalized;
            body.linearVelocity = normalizedDirection * launchSpeed;
            launched = true;
        }

        public void Stop()
        {
            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector2.zero;
            launched = false;
        }

        public void ResetPosition(Vector2 position)
        {
            if (body == null)
            {
                transform.position = position;
                launched = false;
                return;
            }

            body.position = position;
            body.linearVelocity = Vector2.zero;
            launched = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!launched)
            {
                return;
            }

            Paddle paddle = collision.collider.GetComponentInParent<Paddle>();
            if (paddle != null)
            {
                BounceFromPaddle(collision, paddle);
                return;
            }

            MaintainSpeedAfterCollision();
        }

        private void BounceFromPaddle(Collision2D collision, Paddle paddle)
        {
            if (body == null)
            {
                return;
            }

            ContactPoint2D contact = collision.GetContact(0);
            Collider2D paddleCollider = collision.collider;

            float halfWidth = paddleCollider.bounds.extents.x;
            if (halfWidth <= 0.0f)
            {
                halfWidth = 0.5f;
            }

            float offset = (contact.point.x - paddle.transform.position.x) / halfWidth;
            offset = Mathf.Clamp(offset, -1.0f, 1.0f);

            Vector2 direction = new Vector2(offset, 1.0f).normalized;
            body.linearVelocity = direction * launchSpeed;
        }

        private void MaintainSpeedAfterCollision()
        {
            if (body == null)
            {
                return;
            }

            if (body.linearVelocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 direction = body.linearVelocity.normalized;
            float targetSpeed = launchSpeed * wallBounceMultiplier;
            body.linearVelocity = direction * targetSpeed;
        }
    }
}
