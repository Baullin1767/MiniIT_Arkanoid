using Zenject;

namespace MiniIT.ARKANOID
{
    public class UIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<UIController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<HUDView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameOverPanel>().FromComponentInHierarchy().AsSingle();
            Container.Bind<WinPanel>().FromComponentInHierarchy().AsSingle();
        }
    }
}
