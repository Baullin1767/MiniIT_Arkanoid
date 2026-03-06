namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private bool launchQueued = false;

        public float GetMovement()
        {
            if (isLeftPressed == isRightPressed)
            {
                return 0.0f;
            }

            return isLeftPressed ? -1.0f : 1.0f;
        }

        public bool IsLaunchRequested()
        {
            if (!launchQueued)
            {
                return false;
            }

            launchQueued = false;
            return true;
        }

        public void SetLeftPressed(bool isPressed)
        {
            isLeftPressed = isPressed;
        }

        public void SetRightPressed(bool isPressed)
        {
            isRightPressed = isPressed;
        }

        public void RequestLaunch()
        {
            launchQueued = true;
        }
    }
}
