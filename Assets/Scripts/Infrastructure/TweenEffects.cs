using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class TweenEffects
    {
        private readonly Dictionary<Transform, Tween> activeTweens = new Dictionary<Transform, Tween>();
        private readonly Dictionary<Transform, Vector3> initialScales = new Dictionary<Transform, Vector3>();

        public void PlayPulseScale(Transform target, float scaleMultiplier, float duration)
        {
            if (target == null)
            {
                return;
            }

            EnsureInitialScale(target);
            KillTween(target);

            Vector3 baseScale = initialScales[target];
            Vector3 targetScale = baseScale * scaleMultiplier;

            Tween tween = target.DOScale(targetScale, duration)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad)
                .OnKill(() => RestoreScale(target));

            activeTweens[target] = tween;
        }

        public void ResetScale(Transform target)
        {
            if (target == null)
            {
                return;
            }

            KillTween(target);
            RestoreScale(target);
        }

        private void EnsureInitialScale(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (!initialScales.ContainsKey(target))
            {
                initialScales.Add(target, target.localScale);
            }
        }

        private void RestoreScale(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (!initialScales.TryGetValue(target, out var initialScale))
            {
                return;
            }

            target.localScale = initialScale;
        }

        private void KillTween(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (activeTweens.TryGetValue(target, out var tween))
            {
                if (tween != null)
                {
                    tween.Kill();
                }

                activeTweens.Remove(target);
            }
        }
    }
}
