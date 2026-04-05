using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class CompositeInputService : IInputService
    {
        private readonly DesktopInputService desktopInputService;
        private readonly MobileInputService mobileInputService;

        public CompositeInputService(DesktopInputService desktopInputService, MobileInputService mobileInputService)
        {
            this.desktopInputService = desktopInputService;
            this.mobileInputService = mobileInputService;
        }

        public Vector2 GetMoveInput()
        {
            Vector2 mobileInput = mobileInputService != null ? mobileInputService.GetMoveInput() : Vector2.zero;
            if (mobileInput.sqrMagnitude > 0.0f)
            {
                return mobileInput;
            }

            return desktopInputService != null ? desktopInputService.GetMoveInput() : Vector2.zero;
        }

        public bool TryConsumeLaunchDirection(out Vector2 direction)
        {
            direction = Vector2.zero;

            if (mobileInputService != null && mobileInputService.TryConsumeLaunchDirection(out direction))
            {
                return true;
            }

            return desktopInputService != null && desktopInputService.TryConsumeLaunchDirection(out direction);
        }
    }
}
