using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    public class MazeRescueCellView : MonoBehaviour
    {
        [SerializeField]
        private Image image = null;

        public void SetVisible(bool visible)
        {
            if (image == null)
            {
                return;
            }

            image.enabled = visible;
        }

        public void SetColor(Sprite color)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = color;
        }
    }
}
