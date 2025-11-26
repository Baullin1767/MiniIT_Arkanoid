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
            Container.DeclareSignal<ScoreChangedSignal>();
            Container.DeclareSignal<LivesChangedSignal>();
            Container.DeclareSignal<BrickDestroyedSignal>();
            Container.DeclareSignal<BallLostSignal>();
        }
    }
}
