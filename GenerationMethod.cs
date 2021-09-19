using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FD.Generation
{
    /// <summary>
    /// Base class for the generation methode (how road is generated)
    /// </summary>
    public abstract class GenerationMethod
    {        
        /// <summary>
        /// Size of a 1x1 tile in units
        /// </summary>
        public Vector2 tileSize;
        /// <summary>
        /// The audio clip to get sample data from
        /// </summary>
        public AudioClip audioClip;
        /// <summary>
        /// Contains debug values
        /// </summary>
        public TileRoadGeneration.Debug debug;
        /// <summary>
        /// The tileset used for generating
        /// </summary>
        public TileSet tileSet;

        public void Init(Vector2 tileSize, AudioClip audioClip, TileSet tileSet, TileRoadGeneration.Debug debug)
        {
            this.tileSize = tileSize;
            this.audioClip = audioClip;
            this.tileSet = tileSet;
            this.debug = debug;
        }

        public abstract IEnumerator Generate();
    }
}