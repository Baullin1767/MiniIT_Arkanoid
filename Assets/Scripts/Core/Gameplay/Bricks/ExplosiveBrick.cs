namespace MiniIT.ARKANOID
{
    public class ExplosiveBrick : BrickBase
    {
        protected override void OnDestroyed(BrickImpactContext context)
        {
            LevelManager?.QueueExplosion(GridPosition, context);
        }
    }
}
