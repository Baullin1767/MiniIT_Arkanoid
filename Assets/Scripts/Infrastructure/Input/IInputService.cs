using UnityEngine;

namespace MiniIT.ARKANOID
{
    public interface IInputService
    {
        Vector2 GetMoveInput();

        bool TryConsumeLaunchDirection(out Vector2 direction);
    }
}
