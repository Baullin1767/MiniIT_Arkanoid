using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(global::Joystick))]
    public class MobileJoystickInputBridge : MonoBehaviour
    {
        [SerializeField]
        private global::Joystick joystick = null;

        [InjectOptional]
        private MobileInputService mobileInputService = null;

        private void Awake()
        {
            if (joystick == null)
            {
                joystick = GetComponent<global::Joystick>();
            }
        }

        private void Update()
        {
            if (mobileInputService == null || joystick == null)
            {
                return;
            }

            mobileInputService.SetMovement(new Vector2(joystick.Horizontal, joystick.Vertical));
        }

        private void OnDisable()
        {
            if (mobileInputService == null)
            {
                return;
            }

            mobileInputService.SetMovement(Vector2.zero);
        }
    }
}
