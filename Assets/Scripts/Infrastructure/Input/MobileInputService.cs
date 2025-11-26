using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        public float GetMovement()
        {
            if (Input.touchCount == 0)
            {
                return 0.0f;
            }

            Touch touch = Input.GetTouch(0);
            float halfWidth = Screen.width * 0.5f;
            float normalized = (touch.position.x - halfWidth) / halfWidth;

            return Mathf.Clamp(normalized, -1.0f, 1.0f);
        }

        public bool IsLaunchRequested()
        {
            if (Input.touchCount == 0)
            {
                return false;
            }

            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
        }
    }
}
