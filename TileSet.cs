using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FD.Generation
{
    /// <summary>
    /// Contains the tiles from a set and the info about the tiles
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "TileSet_New", menuName = "FD/TileRoadGeneration/TileSet")]
    public class TileSet : ScriptableObject
    {
        public TileSetType type = TileSetType.forest;

        [Header("Special Tiles")]
        [Tooltip("Object created around the tile grid to fill in empty spaces")]
        public GameObject tileFiller;
        public Tile tileStart;
        public Tile tileFinish;

        [Header("Tile groups")]
        /// <summary>
        /// Grouping of similar types with same sampleValue sorting
        /// </summary>
        /// <remarks>
        /// The first group has to have tile 0,1,2 set to straight, curve left, curve right!
        /// </remarks>
        public TileGroup[] groups;

        private void OnValidate()
        {
            // Check if a tile secondsToComplete isnt 0
            for(int i = 0; i < groups.Length; i++) // TODO add more error checking
            {
                for(int j = 0; j < groups[i].tiles.Length; j++)
                {
                    if(groups[i].tiles[j].secondsToComplete <= 0)
                    {
                        Debug.LogError("[TileSet Inspector] TileSet Group: " + i + " has tile: " + j + " secondsToComplete set to <= 0. This is not allowed!");
                        groups[i].tiles[j].secondsToComplete = 1.5f;
                    }
                }
            }
        }
    }
}