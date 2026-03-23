using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    [CreateAssetMenu(fileName = "MazeLayout", menuName = "MiniIT/Arkanoid/Maze Layout", order = 1)]
    public class MazeLayoutAsset : ScriptableObject
    {
        [SerializeField]
        private List<string> rows = new List<string>();

        public IReadOnlyList<string> Rows => rows;

        public bool TryValidate(out string error)
        {
            error = string.Empty;

            if (rows == null || rows.Count == 0)
            {
                error = "Maze layout must contain at least one row.";
                return false;
            }

            int width = -1;
            int startCount = 0;
            int exitCount = 0;

            for (int y = 0; y < rows.Count; y++)
            {
                string row = rows[y];
                if (string.IsNullOrEmpty(row))
                {
                    error = $"Maze layout row {y} is empty.";
                    return false;
                }

                if (width < 0)
                {
                    width = row.Length;
                }
                else if (row.Length != width)
                {
                    error = "Maze layout rows must all have the same width.";
                    return false;
                }

                for (int x = 0; x < row.Length; x++)
                {
                    switch (row[x])
                    {
                        case '#':
                        case '.':
                            break;
                        case 'S':
                            startCount++;
                            break;
                        case 'E':
                            exitCount++;
                            break;
                        default:
                            error = $"Maze layout contains unsupported character '{row[x]}' at ({x}, {y}).";
                            return false;
                    }
                }
            }

            if (startCount != 1)
            {
                error = $"Maze layout must contain exactly one start tile, found {startCount}.";
                return false;
            }

            if (exitCount != 1)
            {
                error = $"Maze layout must contain exactly one exit tile, found {exitCount}.";
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            if (TryValidate(out string error))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogWarning($"MazeLayoutAsset '{name}' is invalid: {error}", this);
            }
        }
    }
}
