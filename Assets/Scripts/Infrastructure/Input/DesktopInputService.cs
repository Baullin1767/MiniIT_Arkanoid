using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class DesktopInputService : IInputService
    {
        private const string HorizontalAxis = "Horizontal";
        private const string LaunchKey = "Jump";

        public float GetMovement()
        {
            float raw = Input.GetAxisRaw(HorizontalAxis);
            return Mathf.Clamp(raw, -1.0f, 1.0f);
        }

        public bool IsLaunchRequested()
        {
            return Input.GetButtonDown(LaunchKey);
        }
    }
}
