using Zenject;

namespace MiniIT.ARKANOID
{
    public class SplitterBrick : BrickBase
    {
        private BallCoordinator ballCoordinator = null;

        [Inject]
        public void Construct(BallCoordinator ballCoordinator)
        {
            this.ballCoordinator = ballCoordinator;
        }

        protected override void OnDestroyed(BrickImpactContext context)
        {
            ballCoordinator?.SpawnSplitterClones(context.SourceBall);
        }
    }
}
