namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        private const float MovementDeadZone = 0.1f;

        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private bool launchQueued = false;
        private float movement;

        public float GetMovement()
        {
            if (movement != 0.0f)
            {
                return movement;
            }

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

        public void SetMovement(float movementValue)
        {
            float clamped = UnityEngine.Mathf.Clamp(movementValue, -1.0f, 1.0f);
            movement = UnityEngine.Mathf.Abs(clamped) < MovementDeadZone ? 0.0f : clamped;
        }
    }
}
