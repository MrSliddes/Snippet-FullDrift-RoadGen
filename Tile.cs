using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FD.Generation
{
    /// <summary>
    /// Contains informatio about a road tile
    /// </summary>
    [System.Serializable]
    public class Tile
    {
        [Tooltip("Name for inspector purposes")]
        [SerializeField] private string inspectorName;

        /// <summary>
        /// The prefab that gets spawned with this tile
        /// </summary>
        [Tooltip("The prefab that gets spawned with this tile")]
        public GameObject prefab;
        /// <summary>
        /// The shape of the road.
        /// </summary>
        /// <example>
        /// (0,0) means a 1x1 tile starting at x = 0 & z = 0. { (0,0), (0,1) } means a tile that is 1x2
        /// </example>
        [Tooltip(" The shape of the road from pivot point locally.\n(0.0,0.5f) means a 1x1 tile starting at x = 0 & z = 0 with center 0.0f, 0.5f. { (0,0), (0,1.5) } means a tile that is 1x2")]
        public Vector2[] shape;
        /// <summary>
        /// The end position (or opening) of the road prefab road.
        /// </summary>
        /// <example>
        /// A straight 1x1 tile has endPosition 0,1. A 1x1 left corner has endPosition -0.5f, 0.5f
        /// </example>
        [Tooltip("The end position (or opening) of the road prefab road.\nA straight 1x1 tile has endPosition 0,1. A 1x1 left corner has endPosition -0.5f, 0.5f. Placed at edge of a tile")]
        public Vector2 endPosition;

        /// <summary>
        /// How much seconds it takes to finish driving over this tile on average. (1.5f sec ~= 1x1 tile)
        /// </summary>
        /// <remarks>
        /// Ideal drift speed is around 50kmh. 50kmh is ~14 meters a sec so a 1x1 tile is about 1.5seconds to drive over
        /// </remarks>
        [Tooltip("How much seconds it takes to finish driving over this tile on average. (1.5f sec ~= 1x1 tile)")]
        public float secondsToComplete = 1.5f;

        [Tooltip("The chance this road tile gets spawned. 1 is lowest, 100 is highest. Important: order tiles based on highest to lowest spawnChance")]
        [Range(1, 100)]
        public int spawnWeight = 50;

        [Header("Direction")]
        /// <summary>
        /// If startDirection != 0 means that the tile is special / can only be connected right horizontally (like U-shaped tile)
        /// </summary>
        [Tooltip("If startDirection != 0 means that the tile is special / can only be connected right horizontally (like U-shaped tile)")]
        public int startDirection = 0;
        /// <summary>
        /// The end direction of the prefab where its going. 0 = straight, -1 = left, 1 = right. If the direction is -1 that means there is a counter part for right
        /// </summary>
        [Tooltip("The end direction of the prefab where its going. 0 = straight, -1 = left, 1 = right. If the direction is -1 that means there is a counter part for right")]
        public int endDirection = 0;

        [Header("Special")]
        /// <summary>
        /// If the tile got a mirror piece (tile gos to the left so mirror tile gos to the right
        /// </summary>
        public GameObject prefabMirror;
        /// <summary>
        /// The index of a tile used as an adjustmentTile.(group index, tiles index) Tiles like U-shaped tiles cannot be added directly to direction 0 so a prefabAdjustmentTile is generated before it and the tile after it to make it fit.
        /// </summary>
        public Vector2Int adjustmentTileIndex = new Vector2Int(-1, -1);

        #region Constructors

        public Tile() { }

        public Tile(Tile t)
        {
            this.prefab = t.prefab;
            this.shape = t.shape;
            this.endPosition = t.endPosition;
            this.secondsToComplete = t.secondsToComplete;
            this.spawnWeight = t.spawnWeight;
            this.endDirection = t.endDirection;
            this.prefabMirror = t.prefabMirror;
            this.adjustmentTileIndex = t.adjustmentTileIndex;
        }

        public Tile(GameObject prefab, Vector2[] shape, Vector2 endPosition, float secondsToComplete, int spawnWeight, int direction, GameObject prefabMirror, Vector2Int adjustmentTileIndex)
        {
            this.prefab = prefab;
            this.shape = shape;
            this.endPosition = endPosition;
            this.secondsToComplete = secondsToComplete;
            this.spawnWeight = spawnWeight;
            this.endDirection = direction;
            this.prefabMirror = prefabMirror;
            this.adjustmentTileIndex = adjustmentTileIndex;
        }

        #endregion
    }
}