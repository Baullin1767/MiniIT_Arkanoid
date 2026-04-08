using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField]
        private Ball[] ballPrefabs = System.Array.Empty<Ball>();

        [SerializeField]
        private Transform spawnPoint = null;

        [SerializeField]
        private Vector2 fallbackSpawnPosition = new Vector2(0.0f, -3.5f);

        private readonly List<Ball> activeBalls = new List<Ball>();
        private readonly Dictionary<int, Ball> prefabByTier = new Dictionary<int, Ball>();

        private DiContainer container = null;
        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private Ball currentBall = null;
        private bool awaitingLaunch = false;
        private bool isMazeActive = false;
        private Coroutine launchCoroutine = null;

        [Inject]
        public void Construct(DiContainer container, IInputService inputService, SignalBus signalBus)
        {
            this.container = container;
            this.inputService = inputService;
            this.signalBus = signalBus;
        }

        private void Awake()
        {
            BuildTierLookup();
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
                launchCoroutine = StartCoroutine(Spawn());
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
            if (awaitingLaunch)
            {
                return;
            }

            Vector2 spawnPosition = GetSpawnPosition();
            currentBall = SpawnBall(GetRandomBallPrefab(), spawnPosition, true);
            awaitingLaunch = currentBall != null;
            if (launchCoroutine!=null)
            {
                StopCoroutine(launchCoroutine);
                launchCoroutine = null;
            }
        }

        private void SpawnMergedBall(int tier, Vector2 position)
        {
            Ball mergedBall = SpawnBall(GetPrefabForTier(tier), position, false);
            mergedBall?.SettleAt(position);
        }

        private void LaunchBall(Vector2 direction)
        {
            if (currentBall == null)
            {
                return;
            }

            Ball launchedBall = currentBall;
            currentBall = null;
            awaitingLaunch = false;
            launchedBall.Launch(direction);
        }

        private void OnBallReset()
        {
            ResetBallSet();
        }

        private void OnBallReachedTop()
        {
            SpawnPendingBall();
        }

        private IEnumerator Spawn()
        {
            yield return new WaitForSeconds(1f);
            SpawnPendingBall();
        }

        private void OnBallContact(BallContactSignal signal)
        {
            Ball source = signal.Source;
            Ball target = signal.Target;

            if (!IsTracked(source) || !IsTracked(target) || source == target /*|| !source.CanTriggerContactEffects()*/)
            {
                return;
            }

            bool shouldSpawnPendingBall = source.TryConsumeSpawnOpportunity();
            if (shouldSpawnPendingBall)
            {
                SpawnPendingBall();
                Debug.Log("OnBallContact");
            }

            if (source.Tier != target.Tier)
            {
                return;
            }

            int nextTier = source.Tier + 1;
            if (GetPrefabForTier(nextTier) == null || !source.CanParticipateInMerge() || !target.CanParticipateInMerge())
            {
                return;
            }

            source.LockForMerge();
            target.LockForMerge();

            Vector2 mergedPosition = ((Vector2)source.transform.position + (Vector2)target.transform.position) * 0.5f;
            RemoveBall(source);
            RemoveBall(target);
            SpawnMergedBall(nextTier, mergedPosition);
            signalBus?.Fire(new BallMergedSignal(nextTier));
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
            signalBus.Subscribe<BallContactSignal>(OnBallContact);
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
            signalBus.Unsubscribe<BallContactSignal>(OnBallContact);
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

        private void BuildTierLookup()
        {
            prefabByTier.Clear();

            if (ballPrefabs == null)
            {
                return;
            }

            for (int i = 0; i < ballPrefabs.Length; i++)
            {
                Ball prefab = ballPrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                prefabByTier[i + 1] = prefab;
            }
        }

        private Vector2 GetSpawnPosition()
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            return fallbackSpawnPosition;
        }

        private Ball SpawnBall(Ball prefab, Vector2 position, bool resetToSpawn)
        {
            if (prefab == null)
            {
                return null;
            }

            GameObject ballObject = container != null
                ? container.InstantiatePrefab(prefab.gameObject)
                : Instantiate(prefab.gameObject);

            if (ballObject == null)
            {
                return null;
            }

            ballObject.transform.SetPositionAndRotation(position, Quaternion.identity);

            Ball ball = ballObject.GetComponent<Ball>();
            if (ball == null)
            {
                Destroy(ballObject);
                return null;
            }

            ball.SetTier(GetTierForPrefab(prefab));
            activeBalls.Add(ball);

            if (resetToSpawn)
            {
                ball.ResetPosition(position);
            }

            return ball;
        }

        private int GetTierForPrefab(Ball prefab)
        {
            foreach (KeyValuePair<int, Ball> entry in prefabByTier)
            {
                if (entry.Value == prefab)
                {
                    return entry.Key;
                }
            }

            return 1;
        }

        private Ball GetPrefabForTier(int tier)
        {
            if (prefabByTier.TryGetValue(tier, out Ball prefab))
            {
                return prefab;
            }

            return null;
        }

        private Ball GetRandomBallPrefab()
        {
            if (ballPrefabs == null || ballPrefabs.Length == 0)
            {
                return null;
            }

            int spawnableCount = Mathf.Min(ballPrefabs.Length, 3);
            int totalWeight = 0;
            for (int i = 0; i < spawnableCount; i++)
            {
                if (ballPrefabs[i] != null)
                {
                    totalWeight += spawnableCount - i;
                }
            }

            if (totalWeight == 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < spawnableCount; i++)
            {
                if (ballPrefabs[i] == null)
                {
                    continue;
                }

                cumulative += spawnableCount - i;
                if (roll < cumulative)
                {
                    return ballPrefabs[i];
                }
            }

            return null;
        }

        private bool IsTracked(Ball ball)
        {
            return ball != null && activeBalls.Contains(ball);
        }

        private void RemoveBall(Ball ball)
        {
            if (ball == null)
            {
                return;
            }

            activeBalls.Remove(ball);
            if (currentBall == ball)
            {
                currentBall = null;
                awaitingLaunch = false;
            }

            ball.Stop();
            Destroy(ball.gameObject);
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
