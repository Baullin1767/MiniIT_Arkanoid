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
        private List<BrickLayoutAsset> layouts = new List<BrickLayoutAsset>();

        private readonly Dictionary<BrickType, Queue<BrickBase>> pool = new Dictionary<BrickType, Queue<BrickBase>>();
        private List<BrickBase> bricks = null;
        private bool poolPrewarmed = false;

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
        }

        public void ResetLevel()
        {
            if (bricksRoot == null)
            {
                bricksRoot = transform;
            }
            PrewarmPool();
            ReturnActiveBricksToPool();

            BrickLayoutAsset layout = PickLayout();
            if (layout == null)
            {
                return;
            }

            SpawnLayout(layout);
        }

        public void RegisterBrick(BrickBase brick)
        {
            if (brick == null)
            {
                return;
            }

            Bricks.Add(brick);
        }

        public void UnregisterBrick(BrickBase brick)
        {
            if (brick == null)
            {
                return;
            }

            Bricks.Remove(brick);
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
                SpawnRow(rows[i]);
            }
        }

        private void SpawnRow(BrickRow row)
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
                SpawnBrick(brickType, position);
            }
        }

        private void SpawnBrick(BrickType brickType, Vector2 position)
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

            brick.gameObject.SetActive(true);
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
            if (container != null)
            {
                return container.InstantiatePrefabForComponent<BrickBase>(prefab);
            }

            BrickBase instance = Instantiate(prefab);
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
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private BrickType ResolveType(BrickBase brick)
        {
            if (brick is ReinforcedBrick)
            {
                return BrickType.Reinforced;
            }

            return BrickType.Standard;
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
    }
}
