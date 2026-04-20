using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniIT.ARKANOID
{
    public class MobileInputService : IInputService
    {
        private const float MovementDeadZone = 0.1f;
        private const float MinimumSwipeDistancePixels = 50.0f;
        private const float MouseDragRangePixels = 120.0f;

        private bool isLeftPressed = false;
        private bool isRightPressed = false;
        private Vector2 joystickMovement = Vector2.zero;
        private bool isSwipeTracking = false;
        private int swipeFingerId = -1;
        private Vector2 swipeStartPosition = Vector2.zero;
        private bool isMouseSwipeTracking = false;
        private Vector2 mouseSwipeStartPosition = Vector2.zero;

        public Vector2 GetMoveInput()
        {
            if (joystickMovement.sqrMagnitude > 0.0f)
            {
                return joystickMovement;
            }

            if (TryGetMouseDragMovement(out Vector2 mouseMovement))
            {
                return mouseMovement;
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

            if (touches.Length == 0)
            {
                return TryConsumeMouseLaunchDirection(out direction);
            }

            if (!isSwipeTracking)
            {
                for (int i = 0; i < touches.Length; i++)
                {
                    Touch touch = touches[i];

                    if (touch.phase != TouchPhase.Began)
                        continue;

                    if (IsPointerOverUi(touch.fingerId))
                        continue;

                    isSwipeTracking = true;
                    swipeFingerId = touch.fingerId;
                    swipeStartPosition = touch.position;
                    break;
                }

                return false;
            }

            for (int i = 0; i < touches.Length; i++)
            {
                Touch touch = touches[i];

                if (touch.fingerId != swipeFingerId)
                    continue;

                if (touch.phase == TouchPhase.Canceled)
                {
                    ResetSwipeTracking();
                    return false;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 swipe = touch.position - swipeStartPosition;
                    ResetSwipeTracking();
                    return TryBuildLaunchDirection(swipe, out direction);
                }

                return false;
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

        private bool TryGetMouseDragMovement(out Vector2 movement)
        {
            movement = Vector2.zero;

            if (Input.touchCount > 0)
            {
                ResetMouseSwipeTracking();
                return false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isMouseSwipeTracking = !IsPointerOverUi();
                mouseSwipeStartPosition = Input.mousePosition;
            }

            if (!isMouseSwipeTracking)
            {
                return false;
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - mouseSwipeStartPosition;
                movement = Vector2.ClampMagnitude(delta / MouseDragRangePixels, 1.0f);
                movement.x = ApplyDeadZone(movement.x);
                movement.y = ApplyDeadZone(movement.y);
                return movement.sqrMagnitude > 0.0f;
            }

            if (!Input.GetMouseButtonUp(0))
            {
                ResetMouseSwipeTracking();
            }

            return false;
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

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void ResetSwipeTracking()
        {
            isSwipeTracking = false;
            swipeFingerId = -1;
            swipeStartPosition = Vector2.zero;
        }

        private bool TryConsumeMouseLaunchDirection(out Vector2 direction)
        {
            direction = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                isMouseSwipeTracking = !IsPointerOverUi();
                mouseSwipeStartPosition = Input.mousePosition;
            }

            if (isMouseSwipeTracking && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0))
            {
                ResetMouseSwipeTracking();
                return false;
            }

            if (!Input.GetMouseButtonUp(0) || !isMouseSwipeTracking)
            {
                return false;
            }

            Vector2 swipe = (Vector2)Input.mousePosition - mouseSwipeStartPosition;
            ResetMouseSwipeTracking();
            return TryBuildLaunchDirection(swipe, out direction);
        }

        private void ResetMouseSwipeTracking()
        {
            isMouseSwipeTracking = false;
            mouseSwipeStartPosition = Vector2.zero;
        }
    }
}
