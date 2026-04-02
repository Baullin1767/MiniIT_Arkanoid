using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallCoordinator
    {
        private const int MaxSimultaneousBalls = 3;
        private const float SplitAngleOffset = 25.0f;
        private const float FallbackCloneSpeed = 8.0f;

        private readonly DiContainer container;
        private readonly SignalBus signalBus;
        private readonly HashSet<Ball> activeBalls = new HashSet<Ball>();
        private readonly HashSet<Ball> cloneBalls = new HashSet<Ball>();

        private Ball primaryBall = null;

        public BallCoordinator(DiContainer container, SignalBus signalBus)
        {
            this.container = container;
            this.signalBus = signalBus;

            signalBus.Subscribe<LevelResetSignal>(ClearClones);
            signalBus.Subscribe<GameOverSignal>(ClearClones);
            signalBus.Subscribe<LevelCompletedSignal>(ClearClones);
        }

        public void SetPrimaryBall(Ball ball)
        {
            primaryBall = ball;
        }

        public void SetBallInPlay(Ball ball, bool isInPlay)
        {
            if (ball == null)
            {
                return;
            }

            if (isInPlay)
            {
                activeBalls.Add(ball);
                return;
            }

            activeBalls.Remove(ball);
        }

        public void HandleBallOutOfBounds(Ball ball)
        {
            if (ball == null)
            {
                return;
            }

            bool isClone = cloneBalls.Contains(ball);
            ball.Stop();

            if (isClone)
            {
                cloneBalls.Remove(ball);
                Object.Destroy(ball.gameObject);
            }

            activeBalls.RemoveWhere(entry => entry == null);
            if (activeBalls.Count == 0)
            {
                signalBus.Fire<BallLostSignal>();
            }
        }

        public void SpawnSplitterClones(Ball sourceBall)
        {
            if (sourceBall == null)
            {
                return;
            }

            Ball templateBall = primaryBall != null ? primaryBall : sourceBall;
            if (templateBall == null)
            {
                return;
            }

            int availableSlots = MaxSimultaneousBalls - activeBalls.Count;
            int spawnCount = Mathf.Min(2, availableSlots);
            if (spawnCount <= 0)
            {
                return;
            }

            float[] angleOffsets = { -SplitAngleOffset, SplitAngleOffset };
            Vector2 sourceDirection = sourceBall.CurrentDirection.sqrMagnitude <= Mathf.Epsilon
                ? Vector2.up
                : sourceBall.CurrentDirection.normalized;
            float sourceSpeed = sourceBall.CurrentSpeed > 0.0f
                ? sourceBall.CurrentSpeed
                : FallbackCloneSpeed;

            for (int i = 0; i < spawnCount; i++)
            {
                Ball clone = container.InstantiatePrefabForComponent<Ball>(templateBall.gameObject);
                clone.transform.SetParent(templateBall.transform.parent, false);
                clone.transform.position = sourceBall.transform.position;
                clone.name = $"{templateBall.name}_Clone";

                cloneBalls.Add(clone);

                Vector2 cloneDirection = (Vector2)(Quaternion.Euler(0.0f, 0.0f, angleOffsets[i]) * sourceDirection);
                clone.Launch(cloneDirection, sourceSpeed);
            }
        }

        private void ClearClones()
        {
            foreach (Ball clone in cloneBalls)
            {
                if (clone == null)
                {
                    continue;
                }

                clone.Stop();
                Object.Destroy(clone.gameObject);
            }

            cloneBalls.Clear();
            activeBalls.RemoveWhere(ball => ball == null || ball != primaryBall);
        }
    }
}
