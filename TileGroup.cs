using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FD.Generation
{
    /// <summary>
    /// Contains tiles with the same sample rate
    /// </summary>
    [System.Serializable]
    public class TileGroup
    {
        /// <summary>
        /// For inspector purposes. Shows around what value these tiles are used
        /// </summary>
        [Tooltip("For inspector purposes. Shows around what value these tiles are used")]
        [SerializeField] private string inspectorName;
        /// <summary>
        /// The tiles used for the sample value connected to this tilegroup
        /// </summary
        [Tooltip("The tiles used for the sample value connected to this tilegroup")]
        public Tile[] tiles;
    }
}