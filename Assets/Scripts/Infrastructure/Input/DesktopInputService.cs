using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class DesktopInputService : IInputService
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";
        private const string LaunchKey = "Jump";

        public Vector2 GetMoveInput()
        {
            float horizontal = Mathf.Clamp(Input.GetAxisRaw(HorizontalAxis), -1.0f, 1.0f);
            float vertical = Mathf.Clamp(Input.GetAxisRaw(VerticalAxis), -1.0f, 1.0f);

            return new Vector2(horizontal, vertical);
        }

        public bool IsLaunchRequested()
        {
            return Input.GetButtonDown(LaunchKey);
        }
    }
}
