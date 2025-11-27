using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public enum BrickType
    {
        Standard = 0,
        Reinforced = 1
    }

    [Serializable]
    public struct BrickRow
    {
        public float YPosition;
        public float StartX;
        public float Spacing;
        public List<BrickType> Bricks;
    }

    [CreateAssetMenu(fileName = "BrickLayout", menuName = "MiniIT/Arkanoid/Brick Layout", order = 0)]
    public class BrickLayoutAsset : ScriptableObject
    {
        [SerializeField]
        private List<BrickRow> rows = new List<BrickRow>();

        public IReadOnlyList<BrickRow> Rows
        {
            get
            {
                return rows;
            }
        }
    }
}
