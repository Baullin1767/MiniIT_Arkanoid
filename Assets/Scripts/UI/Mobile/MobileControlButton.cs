using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum ControlType
        {
            Left = 0,
            Right = 1,
            Launch = 2
        }

        [SerializeField]
        private ControlType controlType = ControlType.Left;

        [InjectOptional]
        private MobileInputService mobileInputService = null;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (mobileInputService == null)
            {
                return;
            }

            if (controlType == ControlType.Launch)
            {
                return;
            }

            SetDirectionalState(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetDirectionalState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetDirectionalState(false);
        }

        private void OnDisable()
        {
            SetDirectionalState(false);
        }

        private void SetDirectionalState(bool isPressed)
        {
            if (mobileInputService == null)
            {
                return;
            }

            if (controlType == ControlType.Left)
            {
                mobileInputService.SetLeftPressed(isPressed);
            }
            else if (controlType == ControlType.Right)
            {
                mobileInputService.SetRightPressed(isPressed);
            }
        }
    }
}
