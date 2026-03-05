using Zenject;

namespace MiniIT.ARKANOID
{
    public class MainMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MainMenuController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<WindowsManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
