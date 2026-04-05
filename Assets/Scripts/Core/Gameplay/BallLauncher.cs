using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField]
        private Ball ballPrefab = null;

        [SerializeField]
        private Transform spawnPoint = null;

        [SerializeField]
        private Vector2 fallbackSpawnPosition = new Vector2(0.0f, -3.5f);

        private DiContainer container = null;
        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private Ball currentBall = null;
        private bool awaitingLaunch = false;
        private bool isMazeActive = false;

        [Inject]
        public void Construct(DiContainer container, IInputService inputService, SignalBus signalBus)
        {
            this.container = container;
            this.inputService = inputService;
            this.signalBus = signalBus;
        }

        private void OnEnable()
        {
            SubscribeSignals();
        }

        private void OnDisable()
        {
            UnsubscribeSignals();
        }

        private void Start()
        {
            ResetBallToSpawn();
        }

        private void Update()
        {
            if (!awaitingLaunch || isMazeActive || currentBall == null)
            {
                return;
            }

            if (inputService != null && inputService.TryConsumeLaunchDirection(out Vector2 launchDirection))
            {
                LaunchBall(launchDirection);
            }
        }

        private void ResetBallToSpawn()
        {
            Vector2 spawnPosition = GetSpawnPosition();

            awaitingLaunch = true;
            DestroyCurrentBall();

            if (ballPrefab == null)
            {
                return;
            }

            currentBall = SpawnBall(spawnPosition);
            currentBall?.ResetPosition(spawnPosition);
        }

        private void LaunchBall(Vector2 direction)
        {
            if (currentBall == null)
            {
                return;
            }

            awaitingLaunch = false;
            currentBall.Launch(direction);
        }

        private void OnBallReset()
        {
            ResetBallToSpawn();
        }

        private void OnLevelReset()
        {
            ResetBallToSpawn();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallResetSignal>(OnBallReset);
            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Subscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void UnsubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<BallResetSignal>(OnBallReset);
            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Unsubscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void OnMazeStarted()
        {
            isMazeActive = true;
        }

        private void OnMazeEnded()
        {
            isMazeActive = false;
        }

        private Vector2 GetSpawnPosition()
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            return fallbackSpawnPosition;
        }

        private Ball SpawnBall(Vector2 position)
        {
            GameObject ballObject = container != null
                ? container.InstantiatePrefab(ballPrefab.gameObject)
                : Instantiate(ballPrefab.gameObject);

            if (ballObject == null)
            {
                return null;
            }

            ballObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            return ballObject.GetComponent<Ball>();
        }

        private void DestroyCurrentBall()
        {
            if (currentBall == null)
            {
                return;
            }

            Destroy(currentBall.gameObject);
            currentBall = null;
        }
    }
}
