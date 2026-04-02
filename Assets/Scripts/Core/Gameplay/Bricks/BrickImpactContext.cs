using UnityEngine;

namespace MiniIT.ARKANOID
{
    public enum BrickImpactCause
    {
        DirectHit = 0,
        Explosion = 1
    }

    public readonly struct BrickImpactContext
    {
        public Ball SourceBall { get; }

        public Vector2 IncomingDirection { get; }

        public BrickImpactCause Cause { get; }

        public BrickImpactContext(Ball sourceBall, Vector2 incomingDirection, BrickImpactCause cause)
        {
            SourceBall = sourceBall;
            IncomingDirection = incomingDirection;
            Cause = cause;
        }

        public static BrickImpactContext DirectHit(Ball sourceBall, Vector2 incomingDirection)
        {
            return new BrickImpactContext(sourceBall, incomingDirection, BrickImpactCause.DirectHit);
        }

        public static BrickImpactContext Explosion(Ball sourceBall, Vector2 incomingDirection)
        {
            return new BrickImpactContext(sourceBall, incomingDirection, BrickImpactCause.Explosion);
        }
    }

    public readonly struct BrickImpactResult
    {
        public static readonly BrickImpactResult None = new BrickImpactResult(false);
        public static readonly BrickImpactResult CollisionConsumed = new BrickImpactResult(true);

        public bool ConsumeCollision { get; }

        public BrickImpactResult(bool consumeCollision)
        {
            ConsumeCollision = consumeCollision;
        }
    }
}
