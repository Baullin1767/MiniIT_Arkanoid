using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Ball : MonoBehaviour
    {
        private float launchSpeed;

        [SerializeField]
        private Rigidbody2D body = null;

        private bool launched = false;
        private bool reachedTop = false;

        private AudioService _audioService;

        [Inject]
        public void Construct(GameSettings gameSettings, AudioService audioService)
        {
            launchSpeed = gameSettings.launchSpeed;
            _audioService = audioService;
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
            if (body == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            launched = true;
            reachedTop = false;
            body.simulated = true;
            body.velocity = direction.normalized * launchSpeed;
            body.angularVelocity = 0.0f;
            body.WakeUp();
            _audioService.PlaySound(AudioService.SoundType.LaunchSound);
        }

        public void Stop()
        {
            launched = false;

            if (body == null)
            {
                return;
            }

            body.velocity = Vector2.zero;
            body.angularVelocity = 0.0f;
            body.simulated = false;
        }

        public void ResetPosition(Vector2 position)
        {
            Stop();
            reachedTop = false;

            if (body != null)
            {
                body.position = position;
            }

            transform.position = position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!launched || collision.collider == null)
            {
                return;
            }

            _audioService.PlaySound(AudioService.SoundType.HitSound);
        }

        public bool TryMarkReachedTop()
        {
            if (reachedTop)
            {
                return false;
            }

            reachedTop = true;
            return true;
        }
    }
}
