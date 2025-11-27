using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class GameInstaller : MonoInstaller
    {

        [SerializeField]
        private GameSettings gameSettings = null;

        public override void InstallBindings()
        {
            Container.BindInstance(ResolveGameSettings()).AsSingle();
            Container.Bind<GameManager>().AsSingle();
            Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TweenEffects>().AsSingle();

            BindInput();
        }

        private GameSettings ResolveGameSettings()
        {
            if (gameSettings != null)
            {
                return gameSettings;
            }

            GameSettings loadedSettings = Resources.Load<GameSettings>("Configs/Game Settings");

            if (loadedSettings == null)
            {
                Debug.LogError("GameInstaller: Game Settings asset not found at Resources/Configs/Game Settings");
                loadedSettings = ScriptableObject.CreateInstance<GameSettings>();
            }

            return loadedSettings;
        }

        private void BindInput()
        {
            if (UseMobileInput())
            {
                Container.Bind<IInputService>().To<MobileInputService>().AsSingle();
            }
            else
            {
                Container.Bind<IInputService>().To<DesktopInputService>().AsSingle();
            }
        }

        private bool UseMobileInput()
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
                return false;
#endif
            return true;
        }
    }
}
