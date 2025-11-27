using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class ReinforcedBrick : BrickBase
    {
        [SerializeField]
        private SpriteRenderer brickRenderer = null;

        [SerializeField]
        private Color damagedColor = Color.red;

        [SerializeField]
        private float pulseScale = 1.15f;

        [SerializeField]
        private float pulseDuration = 0.2f;

        private Color initialColor = Color.white;
        private bool damaged = false;
        private TweenEffects tweenEffects = null;

        [Inject]
        public void Construct(TweenEffects tweenEffects)
        {
            this.tweenEffects = tweenEffects;
        }

        protected override void Awake()
        {
            base.Awake();
            CacheInitialColor();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetVisualState();
        }

        protected override void OnHit(int remainingHealth)
        {
            if (damaged)
            {
                return;
            }

            damaged = true;

            if (brickRenderer != null)
            {
                brickRenderer.color = damagedColor;
            }

            PlayPulse();
        }

        private void CacheInitialColor()
        {
            if (brickRenderer == null)
            {
                return;
            }

            initialColor = brickRenderer.color;
        }

        private void ResetVisualState()
        {
            damaged = false;

            if (brickRenderer != null)
            {
                brickRenderer.color = initialColor;
            }

            ResetScale();
        }

        private void PlayPulse()
        {
            Transform target = ResolvePulseTarget();

            if (tweenEffects == null)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            tweenEffects.PlayPulseScale(target, pulseScale, pulseDuration);
        }

        private void ResetScale()
        {
            Transform target = ResolvePulseTarget();

            if (tweenEffects == null)
            {
                return;
            }

            if (target == null)
            {
                return;
            }

            tweenEffects.ResetScale(target);
        }

        private Transform ResolvePulseTarget()
        {
            if (brickRenderer != null)
            {
                return brickRenderer.transform;
            }

            return transform;
        }
    }
}
