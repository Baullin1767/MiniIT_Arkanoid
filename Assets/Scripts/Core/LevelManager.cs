using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Tracks bricks on the current level and determines completion.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private DiContainer container = null;

        [SerializeField]
        private Transform bricksRoot = null;

        [SerializeField]
        private BrickBase standardBrickPrefab = null;

        [SerializeField]
        private BrickBase reinforcedBrickPrefab = null;

        [SerializeField]
        private BrickBase explosiveBrickPrefab = null;

        [SerializeField]
        private BrickBase teleportBrickPrefab = null;

        [SerializeField]
        private BrickBase splitterBrickPrefab = null;

        [SerializeField]
        private BrickBase bonusBrickPrefab = null;

        [SerializeField]
        private List<BrickLayoutAsset> layouts = new List<BrickLayoutAsset>();

        private readonly Dictionary<BrickType, Queue<BrickBase>> pool = new Dictionary<BrickType, Queue<BrickBase>>();
        private readonly Dictionary<Vector2Int, BrickBase> activeBrickMap = new Dictionary<Vector2Int, BrickBase>();
        private readonly List<TeleportBrick> activeTeleportBricks = new List<TeleportBrick>();
        private readonly Queue<PendingBrickImpact> pendingImpacts = new Queue<PendingBrickImpact>();
        private List<BrickBase> bricks = null;
        private bool poolPrewarmed = false;
        private BrickLayoutAsset currentLayout = null;
        private bool isProcessingImpacts = false;

        private struct PendingBrickImpact
        {
            public BrickBase Brick;
            public BrickImpactContext Context;
            public bool CaptureResult;
        }

        private List<BrickBase> Bricks
        {
            get
            {
                if (bricks == null)
                {
                    bricks = new List<BrickBase>();
                }

                return bricks;
            }
        }

        [Inject]
        public void Construct(DiContainer container)
        {
            this.container = container;
            // Time.timeScale = 0.1f;
        }

        public void ResetLevel(bool reuseCurrentLayout = false)
        {
            if (bricksRoot == null)
            {
                bricksRoot = transform;
            }

            pendingImpacts.Clear();
            PrewarmPool();
            ReturnActiveBricksToPool();

            BrickLayoutAsset layout = reuseCurrentLayout ? currentLayout : PickLayout();
            if (layout == null)
            {
                currentLayout = null;
                return;
            }

            currentLayout = layout;
            SpawnLayout(layout);
        }

        public void RegisterBrick(BrickBase brick)
        {
            if (brick == null)
            {
                return;
            }

            Bricks.Add(brick);
            activeBrickMap[brick.GridPosition] = brick;

            if (brick is TeleportBrick teleportBrick && !activeTeleportBricks.Contains(teleportBrick))
            {
                activeTeleportBricks.Add(teleportBrick);
            }
        }

        public void UnregisterBrick(BrickBase brick)
        {
            if (brick == null)
            {
                return;
            }

            Bricks.Remove(brick);
            activeBrickMap.Remove(brick.GridPosition);

            if (brick is TeleportBrick teleportBrick)
            {
                activeTeleportBricks.Remove(teleportBrick);
            }
        }

        public bool IsLevelComplete()
        {
            return Bricks.Count == 0;
        }

        private BrickLayoutAsset PickLayout()
        {
            int count = layouts.Count;

            if (count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, count);
            return layouts[index];
        }

        private void SpawnLayout(BrickLayoutAsset layout)
        {
            IReadOnlyList<BrickRow> rows = layout.Rows;

            if (rows == null)
            {
                return;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                SpawnRow(rows[i], i);
            }
        }

        private void SpawnRow(BrickRow row, int rowIndex)
        {
            if (row.Bricks == null)
            {
                return;
            }

            float spacing = row.Spacing;
            if (Mathf.Approximately(spacing, 0.0f))
            {
                spacing = 1.0f;
            }

            for (int i = 0; i < row.Bricks.Count; i++)
            {
                BrickType brickType = row.Bricks[i];
                Vector2 position = new Vector2(row.StartX + spacing * i, row.YPosition);
                SpawnBrick(brickType, position, new Vector2Int(i, rowIndex));
            }
        }

        private void SpawnBrick(BrickType brickType, Vector2 position, Vector2Int gridPosition)
        {
            BrickBase prefab = ResolvePrefab(brickType);
            if (prefab == null)
            {
                return;
            }

            BrickBase brick = GetFromPool(brickType, prefab);
            if (brick == null)
            {
                return;
            }

            Transform brickTransform = brick.transform;

            brickTransform.SetParent(bricksRoot, false);
            brickTransform.localPosition = position;
            brickTransform.localRotation = Quaternion.identity;
            brickTransform.localScale = prefab.transform.localScale;

            brick.PrepareForSpawn(brickType, gridPosition);
            brick.gameObject.SetActive(true);
        }

        public BrickImpactResult HandleBrickImpact(BrickBase brick, BrickImpactContext context)
        {
            BrickImpactResult directImpactResult = BrickImpactResult.None;

            EnqueueImpact(brick, context, true);
            ProcessPendingImpacts(ref directImpactResult);

            return directImpactResult;
        }

        public void QueueExplosion(Vector2Int center, BrickImpactContext sourceContext)
        {
            for (int row = center.y - 1; row <= center.y + 1; row++)
            {
                for (int column = center.x - 1; column <= center.x + 1; column++)
                {
                    Vector2Int position = new Vector2Int(column, row);
                    if (!activeBrickMap.TryGetValue(position, out BrickBase brick) || brick == null)
                    {
                        continue;
                    }

                    EnqueueImpact(brick, BrickImpactContext.Explosion(sourceContext.SourceBall, sourceContext.IncomingDirection));
                }
            }

            if (!isProcessingImpacts)
            {
                BrickImpactResult ignoredResult = BrickImpactResult.None;
                ProcessPendingImpacts(ref ignoredResult);
            }
        }

        public bool TryGetTeleportDestination(TeleportBrick source, out TeleportBrick destination)
        {
            destination = null;

            if (source == null)
            {
                return false;
            }

            int availableCount = 0;
            for (int i = 0; i < activeTeleportBricks.Count; i++)
            {
                TeleportBrick candidate = activeTeleportBricks[i];
                if (candidate == null || candidate == source || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                availableCount++;
            }

            if (availableCount == 0)
            {
                return false;
            }

            int selectedIndex = UnityEngine.Random.Range(0, availableCount);
            for (int i = 0; i < activeTeleportBricks.Count; i++)
            {
                TeleportBrick candidate = activeTeleportBricks[i];
                if (candidate == null || candidate == source || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (selectedIndex == 0)
                {
                    destination = candidate;
                    return true;
                }

                selectedIndex--;
            }

            return false;
        }

        private void ReturnActiveBricksToPool()
        {
            if (Bricks.Count == 0)
            {
                return;
            }

            for (int i = Bricks.Count - 1; i >= 0; i--)
            {
                BrickBase brick = Bricks[i];
                if (brick == null)
                {
                    continue;
                }

                brick.gameObject.SetActive(false);
                Enqueue(brick);
            }

            Bricks.Clear();
        }

        private BrickBase GetFromPool(BrickType type, BrickBase prefab)
        {
            Queue<BrickBase> queue = GetQueue(type);

            BrickBase brick = null;

            if (queue.Count > 0)
            {
                brick = queue.Dequeue();
            }

            if (brick == null)
            {
                brick = CreateBrickInstance(prefab);
            }

            return brick;
        }

        private BrickBase CreateBrickInstance(BrickBase prefab)
        {
            BrickBase instance = null;

            if (container != null)
            {
                instance = container.InstantiatePrefabForComponent<BrickBase>(prefab);
            }
            else
            {
                instance = Instantiate(prefab);
            }

            if (instance != null && instance.gameObject.activeSelf)
            {
                instance.gameObject.SetActive(false);
            }

            return instance;
        }

        private void Enqueue(BrickBase brick)
        {
            BrickType type = ResolveType(brick);

            Queue<BrickBase> queue = GetQueue(type);
            queue.Enqueue(brick);
        }

        private BrickBase ResolvePrefab(BrickType type)
        {
            return type switch
            {
                BrickType.Standard => standardBrickPrefab,
                BrickType.Reinforced => reinforcedBrickPrefab,
                BrickType.Explosive => explosiveBrickPrefab,
                BrickType.Teleport => teleportBrickPrefab,
                BrickType.Splitter => splitterBrickPrefab,
                BrickType.Bonus => bonusBrickPrefab,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private BrickType ResolveType(BrickBase brick)
        {
            if (brick == null)
            {
                return BrickType.Standard;
            }

            return brick.AssignedBrickType;
        }

        private Queue<BrickBase> GetQueue(BrickType type)
        {
            if (!pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<BrickBase>();
                pool[type] = queue;
            }

            return queue;
        }

        private void PrewarmPool()
        {
            if (poolPrewarmed)
            {
                return;
            }

            Dictionary<BrickType, int> requiredCounts = CalculateMaxBrickCounts();

            foreach (KeyValuePair<BrickType, int> entry in requiredCounts)
            {
                BrickBase prefab = ResolvePrefab(entry.Key);
                if (prefab == null)
                {
                    continue;
                }

                Queue<BrickBase> queue = GetQueue(entry.Key);
                while (queue.Count < entry.Value)
                {
                    BrickBase brick = CreateBrickInstance(prefab);
                    brick.gameObject.SetActive(false);
                    queue.Enqueue(brick);
                }
            }

            poolPrewarmed = true;
        }

        private Dictionary<BrickType, int> CalculateMaxBrickCounts()
        {
            Dictionary<BrickType, int> counts = new Dictionary<BrickType, int>();

            for (int i = 0; i < layouts.Count; i++)
            {
                BrickLayoutAsset layout = layouts[i];
                if (layout == null || layout.Rows == null)
                {
                    continue;
                }

                IReadOnlyList<BrickRow> rows = layout.Rows;
                for (int r = 0; r < rows.Count; r++)
                {
                    List<BrickType> brickTypes = rows[r].Bricks;
                    if (brickTypes == null)
                    {
                        continue;
                    }

                    for (int b = 0; b < brickTypes.Count; b++)
                    {
                        BrickType type = brickTypes[b];
                        int current;
                        counts.TryGetValue(type, out current);
                        counts[type] = current + 1;
                    }
                }
            }

            return counts;
        }

        private void EnqueueImpact(BrickBase brick, BrickImpactContext context, bool captureResult = false)
        {
            if (brick == null)
            {
                return;
            }

            pendingImpacts.Enqueue(new PendingBrickImpact
            {
                Brick = brick,
                Context = context,
                CaptureResult = captureResult
            });
        }

        private void ProcessPendingImpacts(ref BrickImpactResult directImpactResult)
        {
            if (isProcessingImpacts)
            {
                return;
            }

            isProcessingImpacts = true;

            try
            {
                while (pendingImpacts.Count > 0)
                {
                    PendingBrickImpact pendingImpact = pendingImpacts.Dequeue();
                    BrickBase brick = pendingImpact.Brick;

                    if (brick == null || !brick.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    BrickImpactResult impactResult = brick.HandleImpact(pendingImpact.Context);
                    if (pendingImpact.CaptureResult)
                    {
                        directImpactResult = impactResult;
                    }
                }
            }
            finally
            {
                isProcessingImpacts = false;
            }
        }
    }
}
