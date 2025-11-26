using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.ARKANOID
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreText = null;

        [SerializeField]
        private TMP_Text livesText = null;

        public void SetScore(int value)
        {
            if (scoreText == null)
            {
                return;
            }
            scoreText.text = "score: " + value;
        }

        public void SetLives(int value)
        {
            if (livesText == null)
            {
                return;
            }

            livesText.text = "lives: " + value;
        }
    }
}
