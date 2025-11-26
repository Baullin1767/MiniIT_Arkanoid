using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    public class GameStarter : MonoBehaviour
    {
        private GameManager gameManager = null;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        private void Start()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.StartGame();
        }
    }
}
