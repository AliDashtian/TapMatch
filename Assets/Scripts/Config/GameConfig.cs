using System;
using UnityEngine;

namespace TapMatch.Runtime.Config
{
    /// <summary>
    /// Pairs a tint color with an optional sprite for each matchable type.
    /// When no sprite is assigned, the prefab's default sprite is used.
    /// </summary>
    [Serializable]
    public struct MatchableTypeData
    {
        [Tooltip("Tint color applied to the sprite renderer")]
        public Color color;

        [Tooltip("Optional sprite override — leave empty to use the prefab default")]
        public Sprite sprite;
    }

    /// <summary>
    /// ScriptableObject holding all configurable game parameters.
    /// Easily tweakable from the Inspector without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TapMatch/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [Tooltip("Number of rows (N) in the grid")]
        [Min(1)]
        [SerializeField] private int rows = 8;

        [Tooltip("Number of columns (M) in the grid")]
        [Min(1)]
        [SerializeField] private int columns = 8;

        [Header("Matchables")]
        [Tooltip("Number of distinct colors (P)")]
        [Min(2)]
        [SerializeField] private int colorCount = 5;

        [Tooltip("Visual data for each matchable type (index = color ID)")]
        [SerializeField]
        private MatchableTypeData[] matchableTypes = new[]
        {
            new MatchableTypeData { color = new Color(0.90f, 0.25f, 0.25f) }, // Red
            new MatchableTypeData { color = new Color(0.25f, 0.65f, 0.95f) }, // Blue
            new MatchableTypeData { color = new Color(0.30f, 0.85f, 0.40f) }, // Green
            new MatchableTypeData { color = new Color(0.95f, 0.85f, 0.20f) }, // Yellow
            new MatchableTypeData { color = new Color(0.75f, 0.35f, 0.85f) }, // Purple
        };

        [Header("Board Layout")]
        [Tooltip("Size of each tile cell in world units")]
        [SerializeField] private float cellSize = 1.1f;

        [Header("Animation")]
        [Tooltip("Duration for matchable fall animation in seconds")]
        [SerializeField] private float fallDuration = 0.3f;

        [Tooltip("Duration for matchable removal animation in seconds")]
        [SerializeField] private float removeDuration = 0.15f;

        public int Rows => rows;
        public int Columns => columns;
        public int ColorCount => colorCount;
        public float CellSize => cellSize;
        public float FallDuration => fallDuration;
        public float RemoveDuration => removeDuration;

        public Color GetColor(int colorId)
        {
            if (colorId < 0 || colorId >= matchableTypes.Length)
                return Color.white;

            return matchableTypes[colorId].color;
        }

        /// <summary>
        /// Returns the sprite for a matchable type, or null if none is assigned.
        /// </summary>
        public Sprite GetSprite(int colorId)
        {
            if (colorId < 0 || colorId >= matchableTypes.Length)
                return null;

            return matchableTypes[colorId].sprite;
        }

        private void OnValidate()
        {
            if (matchableTypes != null && matchableTypes.Length != colorCount)
            {
                Debug.LogWarning(
                    $"GameConfig: matchableTypes length ({matchableTypes.Length}) " +
                    $"doesn't match colorCount ({colorCount}). They should be equal.");
            }
        }
    }
}
