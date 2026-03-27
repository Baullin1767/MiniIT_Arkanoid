using System.Collections.Generic;
using Doozy.Runtime.UIManager.Containers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIView))]
    public class MazeRescuePanel : MonoBehaviour
    {
        private const float DefaultMazeDuration = 15.0f;
        private const float MoveDeadZone = 0.2f;
        private const float MoveRepeatDelay = 0.15f;
        private const string MazeResourcesPath = "Configs/Mazes";
        private const int MaxBoardColumns = 11;
        private const int MaxBoardRows = 9;
        private const int ExpectedCellCount = MaxBoardColumns * MaxBoardRows;

        private static readonly Color FloorColor = new Color(0.77f, 0.88f, 0.95f, 0.95f);
        private static readonly Color WallColor = new Color(0.17f, 0.24f, 0.31f, 0.0f);
        private static readonly Color ExitColor = new Color(0.96f, 0.76f, 0.24f, 1.0f);
        private static readonly Color PlayerColor = new Color(0.25f, 0.88f, 0.44f, 1.0f);

        [SerializeField]
        private List<MazeLayoutAsset> layouts = new List<MazeLayoutAsset>();
        [SerializeField]
        private TMP_Text timerText = null;
        [SerializeField]
        private Transform boardRoot = null;
        [SerializeField]
        private List<MazeRescueCellView> cellViews = new List<MazeRescueCellView>();

        private readonly List<MazeLayoutAsset> runtimeLayouts = new List<MazeLayoutAsset>();

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private UIView view = null;

        private char[][] mazeGrid = null;
        private Vector2Int playerPosition;
        private Vector2Int exitPosition;
        private Vector2Int lastMoveDirection = Vector2Int.zero;

        private bool isActive = false;
        private float remainingTime = 0.0f;
        private float nextMoveTime = 0.0f;

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
        }

        private void Awake()
        {
            view = GetComponent<UIView>();
            ResolveSceneReferences();
            RebuildBoardLayout();
            CacheCellViews();
            UpdateTimerText(0.0f);
            ClearBoard();
            view?.InstantHide();
        }

        private void OnEnable()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<LevelResetSignal>(HideImmediate);
            signalBus.Subscribe<GameOverSignal>(HideImmediate);
            signalBus.Subscribe<LevelCompletedSignal>(HideImmediate);
        }

        private void OnDisable()
        {
            HideImmediate();

            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<LevelResetSignal>(HideImmediate);
            signalBus.Unsubscribe<GameOverSignal>(HideImmediate);
            signalBus.Unsubscribe<LevelCompletedSignal>(HideImmediate);
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;
            UpdateTimerText(Mathf.Max(remainingTime, 0.0f));
            if (remainingTime <= 0.0f)
            {
                FailAttempt();
                return;
            }

            HandleMovement();
        }

        public void HideImmediate()
        {
            isActive = false;
            mazeGrid = null;
            lastMoveDirection = Vector2Int.zero;
            remainingTime = 0.0f;
            nextMoveTime = 0.0f;

            UpdateTimerText(0.0f);
            ClearBoard();
            view?.InstantHide();
        }

        private void OnMazeStarted()
        {
            ActivateMaze();
        }

        private void ActivateMaze()
        {
            HideImmediate();

            if (!TryBuildMaze(out char[][] grid, out Vector2Int startPosition, out Vector2Int endPosition))
            {
                signalBus?.Fire<MazeFailedSignal>();
                return;
            }

            mazeGrid = grid;
            playerPosition = startPosition;
            exitPosition = endPosition;
            remainingTime = DefaultMazeDuration;
            nextMoveTime = Time.unscaledTime;
            isActive = true;

            UpdateTimerText(remainingTime);
            RebuildBoardLayout();
            RefreshBoard();
            view?.Show();
        }

        private void HandleMovement()
        {
            if (inputService == null)
            {
                return;
            }

            Vector2 input = inputService.GetMoveInput();
            if (input.sqrMagnitude < MoveDeadZone * MoveDeadZone)
            {
                lastMoveDirection = Vector2Int.zero;
                return;
            }

            Vector2Int direction = ResolveDirection(input);
            bool directionChanged = direction != lastMoveDirection;
            if (!directionChanged && Time.unscaledTime < nextMoveTime)
            {
                return;
            }

            lastMoveDirection = direction;
            nextMoveTime = Time.unscaledTime + MoveRepeatDelay;
            TryMove(direction);
        }

        private Vector2Int ResolveDirection(Vector2 input)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            {
                return input.x >= 0.0f ? Vector2Int.right : Vector2Int.left;
            }

            return input.y >= 0.0f ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
        }

        private void TryMove(Vector2Int direction)
        {
            Vector2Int nextPosition = playerPosition + direction;

            if (nextPosition.y < 0 || nextPosition.y >= mazeGrid.Length)
            {
                return;
            }

            if (nextPosition.x < 0 || nextPosition.x >= mazeGrid[nextPosition.y].Length)
            {
                return;
            }

            if (mazeGrid[nextPosition.y][nextPosition.x] == '#')
            {
                return;
            }

            playerPosition = nextPosition;
            RefreshBoard();
            if (playerPosition == exitPosition)
            {
                CompleteAttempt();
            }
        }

        private void CompleteAttempt()
        {
            if (!isActive)
            {
                return;
            }

            HideImmediate();
            signalBus?.Fire<MazeCompletedSignal>();
        }

        private void FailAttempt()
        {
            if (!isActive)
            {
                return;
            }

            HideImmediate();
            signalBus?.Fire<MazeFailedSignal>();
        }

        private bool TryBuildMaze(out char[][] grid, out Vector2Int startPosition, out Vector2Int endPosition)
        {
            grid = null;
            startPosition = Vector2Int.zero;
            endPosition = Vector2Int.zero;

            List<MazeLayoutAsset> availableLayouts = GetAvailableLayouts();
            if (availableLayouts.Count == 0)
            {
                Debug.LogError("MazeRescuePanel: no maze layouts are available.");
                return false;
            }

            int startIndex = Random.Range(0, availableLayouts.Count);
            for (int offset = 0; offset < availableLayouts.Count; offset++)
            {
                MazeLayoutAsset layout = availableLayouts[(startIndex + offset) % availableLayouts.Count];
                if (layout == null)
                {
                    continue;
                }

                if (!layout.TryValidate(out string error))
                {
                    Debug.LogWarning($"MazeRescuePanel: skipped invalid maze layout '{layout.name}': {error}", layout);
                    continue;
                }

                IReadOnlyList<string> rows = layout.Rows;
                if (rows.Count > MaxBoardRows || rows[0].Length > MaxBoardColumns)
                {
                    Debug.LogWarning(
                        $"MazeRescuePanel: skipped maze layout '{layout.name}' because it exceeds the authored board size of {MaxBoardColumns}x{MaxBoardRows}.",
                        layout);
                    continue;
                }

                char[][] parsedGrid = new char[rows.Count][];
                Vector2Int parsedStart = Vector2Int.zero;
                Vector2Int parsedExit = Vector2Int.zero;

                for (int y = 0; y < rows.Count; y++)
                {
                    parsedGrid[y] = rows[y].ToCharArray();

                    for (int x = 0; x < parsedGrid[y].Length; x++)
                    {
                        if (parsedGrid[y][x] == 'S')
                        {
                            parsedStart = new Vector2Int(x, y);
                            parsedGrid[y][x] = '.';
                        }
                        else if (parsedGrid[y][x] == 'E')
                        {
                            parsedExit = new Vector2Int(x, y);
                            parsedGrid[y][x] = '.';
                        }
                    }
                }

                grid = parsedGrid;
                startPosition = parsedStart;
                endPosition = parsedExit;
                return true;
            }

            Debug.LogError("MazeRescuePanel: unable to build a playable maze from the available layouts.");
            return false;
        }

        private List<MazeLayoutAsset> GetAvailableLayouts()
        {
            runtimeLayouts.Clear();

            for (int i = 0; i < layouts.Count; i++)
            {
                if (layouts[i] != null)
                {
                    runtimeLayouts.Add(layouts[i]);
                }
            }

            if (runtimeLayouts.Count > 0)
            {
                return runtimeLayouts;
            }

            MazeLayoutAsset[] loadedLayouts = Resources.LoadAll<MazeLayoutAsset>(MazeResourcesPath);
            for (int i = 0; i < loadedLayouts.Length; i++)
            {
                if (loadedLayouts[i] != null)
                {
                    runtimeLayouts.Add(loadedLayouts[i]);
                }
            }

            return runtimeLayouts;
        }

        private void CacheCellViews()
        {
            if (cellViews.Count > 0)
            {
                cellViews.RemoveAll(viewEntry => viewEntry == null);
            }

            if (cellViews.Count == 0 && boardRoot != null)
            {
                cellViews.AddRange(boardRoot.GetComponentsInChildren<MazeRescueCellView>(true));
            }

            cellViews.Sort((left, right) =>
                left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex()));

            if (cellViews.Count > 0 && cellViews.Count != ExpectedCellCount)
            {
                Debug.LogWarning(
                    $"MazeRescuePanel: expected {ExpectedCellCount} board cells but found {cellViews.Count}.",
                    this);
            }
        }

        private void ResolveSceneReferences()
        {
            if (timerText == null)
            {
                Transform timerTransform = transform.Find("TimerText");
                if (timerTransform != null)
                {
                    timerText = timerTransform.GetComponent<TMP_Text>();
                }
            }

            if (boardRoot == null)
            {
                Transform boardTransform = transform.Find("BoardGrid");
                if (boardTransform != null)
                {
                    boardRoot = boardTransform;
                }
            }
        }

        private void RebuildBoardLayout()
        {
            if (boardRoot is RectTransform boardRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(boardRect);
            }
        }

        private void RefreshBoard()
        {
            if (mazeGrid == null || cellViews.Count == 0)
            {
                ClearBoard();
                return;
            }

            int rows = mazeGrid.Length;
            int columns = mazeGrid[0].Length;
            int rowOffset = (MaxBoardRows - rows) / 2;
            int columnOffset = (MaxBoardColumns - columns) / 2;

            for (int index = 0; index < cellViews.Count; index++)
            {
                int boardRow = index / MaxBoardColumns;
                int boardColumn = index % MaxBoardColumns;
                int layoutRow = boardRow - rowOffset;
                int layoutColumn = boardColumn - columnOffset;

                bool insideLayout = layoutRow >= 0 &&
                                    layoutRow < rows &&
                                    layoutColumn >= 0 &&
                                    layoutColumn < columns;

                if (!insideLayout)
                {
                    cellViews[index].SetVisible(false);
                    continue;
                }

                Color cellColor = mazeGrid[layoutRow][layoutColumn] == '#'
                    ? WallColor
                    : FloorColor;

                if (exitPosition.x == layoutColumn && exitPosition.y == layoutRow)
                {
                    cellColor = ExitColor;
                }

                if (playerPosition.x == layoutColumn && playerPosition.y == layoutRow)
                {
                    cellColor = PlayerColor;
                }

                cellViews[index].SetVisible(true);
                cellViews[index].SetColor(cellColor);
            }
        }

        private void ClearBoard()
        {
            for (int i = 0; i < cellViews.Count; i++)
            {
                if (cellViews[i] == null)
                {
                    continue;
                }

                cellViews[i].SetVisible(false);
            }
        }

        private void UpdateTimerText(float timeValue)
        {
            if (timerText == null)
            {
                return;
            }

            timerText.SetText("Time Left: {0:0.0}s", timeValue);
        }
    }
}
