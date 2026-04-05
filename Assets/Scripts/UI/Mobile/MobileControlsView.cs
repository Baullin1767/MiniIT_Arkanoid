using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class MobileControlsView : MonoBehaviour
    {
        [SerializeField]
        private GameObject controlsRoot = null;

        [InjectOptional]
        private IInputService inputService = null;

        private void Awake()
        {
            if (controlsRoot == null)
            {
                controlsRoot = gameObject;
            }

            ApplyVisibility();
        }

        private void OnEnable()
        {
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (controlsRoot == null)
            {
                return;
            }

            bool isSupportedPlatform = Application.isEditor || Application.isMobilePlatform;
            bool isMobileInput = inputService is MobileInputService;
            controlsRoot.SetActive(isSupportedPlatform && isMobileInput);
        }
    }
}
