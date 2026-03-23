using System;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    public class BallSkinApplier : MonoBehaviour
    {
        private const string PlayerSkinKey = "PlayerSkin";

        [SerializeField]
        private SpriteRenderer targetRenderer = null;

        [SerializeField]
        private Sprite[] ballSkins = Array.Empty<Sprite>();

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            ApplySavedSkin();
        }

        public void ApplySavedSkin()
        {
            if (targetRenderer == null || ballSkins == null || ballSkins.Length == 0)
            {
                return;
            }

            int index = Mathf.Clamp(PlayerPrefs.GetInt(PlayerSkinKey, 0), 0, ballSkins.Length - 1);
            Sprite selectedSkin = ballSkins[index];

            if (selectedSkin == null)
            {
                return;
            }

            targetRenderer.sprite = selectedSkin;
        }
    }
}
