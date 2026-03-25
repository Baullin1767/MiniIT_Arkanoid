using System.Collections.Generic;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    public class MazeRescuePanel : MonoBehaviour
    {
        private const float DefaultMazeDuration = 15.0f;
        private const float MoveDeadZone = 0.2f;
        private const float MoveRepeatDelay = 0.15f;
        private const string MazeResourcesPath = "Configs/Mazes";
        private const string PanelBackgroundTextureAssetPath = "Assets/PNG/pause/bg.png";

        [SerializeField]
        private List<MazeLayoutAsset> layouts = new List<MazeLayoutAsset>();
        [SerializeField]
        private Texture2D panelBackgroundTexture = null;

        private readonly List<MazeLayoutAsset> runtimeLayouts = new List<MazeLayoutAsset>();

        private IInputService inputService = null;
        private SignalBus signalBus = null;

        private char[][] mazeGrid = null;
        private Vector2Int playerPosition;
        private Vector2Int exitPosition;
        private Vector2Int lastMoveDirection = Vector2Int.zero;

        private bool isActive = false;
        private float remainingTime = 0.0f;
        private float nextMoveTime = 0.0f;

        private GUIStyle titleStyle = null;
        private GUIStyle bodyStyle = null;
        private GUIStyle timerStyle = null;

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (panelBackgroundTexture != null)
            {
                return;
            }

            panelBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(PanelBackgroundTextureAssetPath);
            if (panelBackgroundTexture != null)
            {
                EditorUtility.SetDirty(this);
            }
        }
#endif

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;
            if (remainingTime <= 0.0f)
            {
                FailAttempt();
                return;
            }

            HandleMovement();
        }

        private void OnGUI()
        {
            if (!isActive || mazeGrid == null || mazeGrid.Length == 0)
            {
                return;
            }

            EnsureStyles();

            Rect screenRect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
            DrawRect(screenRect, new Color(0.02f, 0.05f, 0.12f, 0f));

            float panelWidth = Mathf.Min(Screen.width * 0.90f, 1350.0f);
            float panelHeight = Mathf.Min(Screen.height * 0.9f, 900.0f);
            Rect panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            if (panelBackgroundTexture != null)
            {
                GUI.DrawTexture(panelRect, panelBackgroundTexture, ScaleMode.StretchToFill, true);
                // DrawOutline(panelRect, new Color(0.25f, 0.14f, 0.06f, 1.0f), 4.0f);
            }
            else
            {
                DrawRect(panelRect, new Color(0.6666667f, 0.3764706f, 0.1568628f, 1f));
            }

            Rect titleRect = new Rect(panelRect.x + 20.0f, panelRect.y + 25.0f, panelRect.width - 40.0f, 40.0f);
            GUI.Label(titleRect, "Maze Rescue", titleStyle);

            Rect bodyRect = new Rect(panelRect.x + 20.0f, panelRect.y + 50.0f, panelRect.width - 20.0f, 67.0f);
            GUI.Label(bodyRect, "Reach the exit before time runs out to recover 1 life. Use the joystick to move.", bodyStyle);

            Rect timerRect = new Rect(panelRect.x + 24.0f, panelRect.y + 110.0f, panelRect.width - 48.0f, 36.0f);
            GUI.Label(timerRect, $"Time Left: {remainingTime:0.0}s", timerStyle);

            DrawMaze(panelRect);
        }

        public void HideImmediate()
        {
            isActive = false;
            mazeGrid = null;
            lastMoveDirection = Vector2Int.zero;
            remainingTime = 0.0f;
            nextMoveTime = 0.0f;
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

        private void DrawMaze(Rect panelRect)
        {
            int rows = mazeGrid.Length;
            int columns = mazeGrid[0].Length;

            float availableWidth = panelRect.width - 70.0f;
            float availableHeight = panelRect.height - 210.0f;
            float cellSize = Mathf.Max(18.0f, Mathf.Min(availableWidth / columns, availableHeight / rows));
            float boardWidth = cellSize * columns;
            float boardHeight = cellSize * rows;

            Rect boardRect = new Rect(
                panelRect.x + (panelRect.width - boardWidth) * 0.5f,
                panelRect.y + panelRect.height - boardHeight - 60.0f,
                boardWidth,
                boardHeight);

            DrawRect(boardRect, new Color(0.13f, 0.2f, 0.27f, 1.0f));
            GUI.Box(boardRect, GUIContent.none);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Rect cellRect = new Rect(
                        boardRect.x + x * cellSize + 1.0f,
                        boardRect.y + y * cellSize + 1.0f,
                        cellSize - 2.0f,
                        cellSize - 2.0f);

                    Color cellColor = mazeGrid[y][x] == '#'
                        ? new Color(0.17f, 0.24f, 0.31f, 1.0f)
                        : new Color(0.77f, 0.88f, 0.95f, 0.95f);

                    if (new Vector2Int(x, y) == exitPosition)
                    {
                        cellColor = new Color(0.96f, 0.76f, 0.24f, 1.0f);
                    }

                    if (new Vector2Int(x, y) == playerPosition)
                    {
                        cellColor = new Color(0.25f, 0.88f, 0.44f, 1.0f);
                    }

                    DrawRect(cellRect, cellColor);
                }
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.042f, 22.0f, 34.0f))
                };
                titleStyle.normal.textColor = Color.white;
            }

            if (bodyStyle == null)
            {
                bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.06f, 14.0f, 35.0f))
                };
                bodyStyle.normal.textColor = new Color(0.86f, 0.93f, 0.98f, 1.0f);
            }

            if (timerStyle == null)
            {
                timerStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.03f, 16.0f, 24.0f))
                };
                timerStyle.normal.textColor = new Color(0.99f, 0.92f, 0.65f, 1.0f);
            }
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawOutline(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }
    }
}
