using System.Collections.Generic;
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

        private readonly List<Ball> activeBalls = new List<Ball>();

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
            ResetBallSet();
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

        private void ResetBallSet()
        {
            awaitingLaunch = false;
            DestroyAllBalls();
            SpawnPendingBall();
        }

        private void SpawnPendingBall()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            currentBall = SpawnBall(spawnPosition);
            currentBall?.ResetPosition(spawnPosition);
            awaitingLaunch = currentBall != null;
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
            ResetBallSet();
        }

        private void OnBallReachedTop()
        {
            SpawnPendingBall();
        }

        private void OnLevelReset()
        {
            ResetBallSet();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallResetSignal>(OnBallReset);
            signalBus.Subscribe<BallReachedTopSignal>(OnBallReachedTop);
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
            signalBus.Unsubscribe<BallReachedTopSignal>(OnBallReachedTop);
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

            Ball ball = ballObject.GetComponent<Ball>();
            if (ball != null)
            {
                activeBalls.Add(ball);
            }

            return ball;
        }

        private void DestroyAllBalls()
        {
            for (int i = 0; i < activeBalls.Count; i++)
            {
                Ball ball = activeBalls[i];
                if (ball == null)
                {
                    continue;
                }

                Destroy(ball.gameObject);
            }

            activeBalls.Clear();
            currentBall = null;
        }
    }
}
