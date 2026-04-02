using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniIT.ARKANOID
{
    public class DesktopInputService : IInputService
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";
        private const float MinimumSwipeDistancePixels = 50.0f;

        private bool isSwipeTracking = false;
        private Vector2 swipeStartPosition = Vector2.zero;

        public Vector2 GetMoveInput()
        {
            float horizontal = Mathf.Clamp(Input.GetAxisRaw(HorizontalAxis), -1.0f, 1.0f);
            float vertical = Mathf.Clamp(Input.GetAxisRaw(VerticalAxis), -1.0f, 1.0f);

            return new Vector2(horizontal, vertical);
        }

        public bool TryConsumeLaunchDirection(out Vector2 direction)
        {
            direction = Vector2.zero;

            if (isSwipeTracking && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0))
            {
                isSwipeTracking = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isSwipeTracking = !IsPointerOverUi();
                swipeStartPosition = Input.mousePosition;
            }

            if (!Input.GetMouseButtonUp(0))
            {
                return false;
            }

            if (!isSwipeTracking)
            {
                return false;
            }

            isSwipeTracking = false;
            Vector2 swipe = (Vector2)Input.mousePosition - swipeStartPosition;
            return TryBuildLaunchDirection(swipe, out direction);
        }

        private static bool TryBuildLaunchDirection(Vector2 swipe, out Vector2 direction)
        {
            direction = Vector2.zero;

            if (swipe.magnitude < MinimumSwipeDistancePixels)
            {
                return false;
            }

            Vector2 normalizedDirection = swipe.normalized;
            if (normalizedDirection.y <= 0.0f)
            {
                return false;
            }

            direction = normalizedDirection;
            return true;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
