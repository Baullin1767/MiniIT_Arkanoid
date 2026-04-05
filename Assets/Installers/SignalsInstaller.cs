using Zenject;

namespace MiniIT.ARKANOID
{
    public class SignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<GameOverSignal>();
            Container.DeclareSignal<LevelCompletedSignal>();
            Container.DeclareSignal<LevelResetSignal>();
            Container.DeclareSignal<MazeStartedSignal>();
            Container.DeclareSignal<MazeCompletedSignal>();
            Container.DeclareSignal<MazeFailedSignal>();
            Container.DeclareSignal<ScoreChangedSignal>();
            Container.DeclareSignal<LivesChangedSignal>();
            Container.DeclareSignal<BrickDestroyedSignal>();
            Container.DeclareSignal<BallResetSignal>();
            Container.DeclareSignal<BallLostSignal>();
            Container.DeclareSignal<BallMergedSignal>();
            Container.DeclareSignal<BallReachedTopSignal>();
            Container.DeclareSignal<BallContactSignal>();
            Container.DeclareSignal<BrickFieldReachedBottomSignal>();
        }
    }
}
