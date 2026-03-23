using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        private const float MovementDeadZone = 0.1f;

        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private bool launchQueued = false;
        private Vector2 joystickMovement = Vector2.zero;

        public Vector2 GetMoveInput()
        {
            if (joystickMovement.sqrMagnitude > 0.0f)
            {
                return joystickMovement;
            }

            if (isLeftPressed == isRightPressed)
            {
                return Vector2.zero;
            }

            float horizontal = isLeftPressed ? -1.0f : 1.0f;
            return new Vector2(horizontal, 0.0f);
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

        public void SetMovement(Vector2 movementValue)
        {
            Vector2 clamped = Vector2.ClampMagnitude(movementValue, 1.0f);
            clamped.x = ApplyDeadZone(clamped.x);
            clamped.y = ApplyDeadZone(clamped.y);

            joystickMovement = clamped;
        }

        private float ApplyDeadZone(float value)
        {
            return Mathf.Abs(value) < MovementDeadZone ? 0.0f : value;
        }
    }
}
