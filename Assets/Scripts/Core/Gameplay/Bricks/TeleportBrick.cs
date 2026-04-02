namespace MiniIT.ARKANOID
{
    public class TeleportBrick : BrickBase
    {
        protected override BrickImpactResult BeforeImpact(BrickImpactContext context, ref bool shouldApplyDamage)
        {
            if (context.Cause != BrickImpactCause.DirectHit || context.SourceBall == null)
            {
                return BrickImpactResult.None;
            }

            if (LevelManager == null || !LevelManager.TryGetTeleportDestination(this, out TeleportBrick destination))
            {
                shouldApplyDamage = false;
                return BrickImpactResult.None;
            }

            context.SourceBall.TeleportTo(destination);
            shouldApplyDamage = false;
            DestroyBrick(context);

            return BrickImpactResult.CollisionConsumed;
        }
    }
}
