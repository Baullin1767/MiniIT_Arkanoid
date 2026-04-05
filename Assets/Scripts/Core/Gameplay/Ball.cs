using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Ball : MonoBehaviour
    {
        private const float ContactEffectMinSpeed = 0.05f;

        private float launchSpeed;

        [SerializeField]
        private Rigidbody2D body = null;

        private SignalBus signalBus = null;
        private bool launched = false;
        private bool spawnOpportunityConsumed = false;
        private bool mergeLocked = false;
        private int tier = 1;

        private AudioService _audioService;

        public int Tier => tier;

        [Inject]
        public void Construct(GameSettings gameSettings, AudioService audioService, SignalBus signalBus)
        {
            launchSpeed = gameSettings.launchSpeed;
            _audioService = audioService;
            this.signalBus = signalBus;
        }

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            Stop();
        }

        public void SetTier(int value)
        {
            tier = Mathf.Max(1, value);
        }

        public void Launch(Vector2 direction)
        {
            if (body == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            launched = true;
            spawnOpportunityConsumed = false;
            mergeLocked = false;
            body.simulated = true;
            body.velocity = direction.normalized * launchSpeed;
            body.angularVelocity = 0.0f;
            body.WakeUp();
            _audioService.PlaySound(AudioService.SoundType.LaunchSound);
        }

        public void Stop()
        {
            launched = false;
            spawnOpportunityConsumed = false;
            mergeLocked = false;

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

            if (body != null)
            {
                body.position = position;
            }

            transform.position = position;
        }

        public void SettleAt(Vector2 position)
        {
            launched = false;
            spawnOpportunityConsumed = true;
            mergeLocked = false;

            if (body != null)
            {
                body.simulated = true;
                body.position = position;
                body.velocity = Vector2.zero;
                body.angularVelocity = 0.0f;
                body.Sleep();
            }

            transform.position = position;
        }

        public bool TryConsumeSpawnOpportunity()
        {
            if (spawnOpportunityConsumed)
            {
                return false;
            }

            spawnOpportunityConsumed = true;
            return true;
        }

        public bool CanTriggerContactEffects()
        {
            if (mergeLocked || !launched || body == null || !body.simulated)
            {
                return false;
            }

            return body.velocity.sqrMagnitude >= ContactEffectMinSpeed * ContactEffectMinSpeed;
        }

        public bool CanParticipateInMerge()
        {
            return !mergeLocked;
        }

        public void LockForMerge()
        {
            mergeLocked = true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider == null)
            {
                return;
            }

            Ball otherBall = collision.collider.GetComponentInParent<Ball>();
            if (otherBall != null && otherBall != this && CanTriggerContactEffects())
            {
                signalBus?.Fire(new BallContactSignal(this, otherBall));
            }

            if (launched)
            {
                _audioService.PlaySound(AudioService.SoundType.HitSound);
            }
        }

        public bool TryMarkReachedTop()
        {
            return TryConsumeSpawnOpportunity();
        }
    }
}
