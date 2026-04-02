using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        private const float MovementDeadZone = 0.1f;
        private const float MinimumSwipeDistancePixels = 50.0f;

        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private Vector2 joystickMovement = Vector2.zero;
        private bool isSwipeTracking = false;
        private int swipeFingerId = -1;
        private Vector2 swipeStartPosition = Vector2.zero;

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

        public bool TryConsumeLaunchDirection(out Vector2 direction)
        {
            direction = Vector2.zero;

            Touch[] touches = Input.touches;
            bool trackedFingerFound = false;

            for (int i = 0; i < touches.Length; i++)
            {
                Touch touch = touches[i];

                if (touch.phase == TouchPhase.Began && !isSwipeTracking)
                {
                    if (IsPointerOverUi(touch.fingerId))
                    {
                        continue;
                    }

                    isSwipeTracking = true;
                    swipeFingerId = touch.fingerId;
                    swipeStartPosition = touch.position;
                    continue;
                }

                if (!isSwipeTracking || touch.fingerId != swipeFingerId)
                {
                    continue;
                }

                trackedFingerFound = true;

                if (touch.phase == TouchPhase.Canceled)
                {
                    ResetSwipeTracking();
                    return false;
                }

                if (touch.phase != TouchPhase.Ended)
                {
                    continue;
                }

                Vector2 swipe = touch.position - swipeStartPosition;
                ResetSwipeTracking();
                return TryBuildLaunchDirection(swipe, out direction);
            }

            if (isSwipeTracking && !trackedFingerFound)
            {
                ResetSwipeTracking();
            }

            return false;
        }

        public void SetLeftPressed(bool isPressed)
        {
            isLeftPressed = isPressed;
        }

        public void SetRightPressed(bool isPressed)
        {
            isRightPressed = isPressed;
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

        private static bool IsPointerOverUi(int fingerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        private void ResetSwipeTracking()
        {
            isSwipeTracking = false;
            swipeFingerId = -1;
            swipeStartPosition = Vector2.zero;
        }
    }
}
