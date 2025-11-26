using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Root Settings")]
        public int defaultLives = 5;
        [Header("Paddle Settings")]
        public float speed = 20.0f;
        public float limitX = 7.5f;
        [Header("Ball Settings")]
        public float launchSpeed = 8.0f;
        public float wallBounceMultiplier = 0.9f;
    }
}