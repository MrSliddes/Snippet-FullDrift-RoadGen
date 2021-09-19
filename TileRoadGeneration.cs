using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FD.Generation
{
    [RequireComponent(typeof(AudioSource))]
    public class TileRoadGeneration : MonoBehaviour
    {

        [Tooltip("The size of a 1:1 tile")]
        public Vector2 tileSize = new Vector2(20, 20);

        [Header("Audio Components")]
        public AudioClip audioClip;
        public AudioClip audioClipFiller;
        public AudioSource audioSource;

        [Header("Debug")]
        public Debug debug;

        private TileSet tileSet_Forest;
        private GenerationMethod generationMethod = new GenerationMethodTiles();

        // Start is called before the first frame update
        void Start()
        {
            // Get
            tileSet_Forest = Resources.Load<TileSet>("TileSet_Forest");
            if(audioSource == null) audioSource = GetComponent<AudioSource>();
            if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            // Check if GameRules is set
            if(FindObjectOfType<GameRules>() != null)
            {
                UnityEngine.Debug.Log("[Road Generation] Detected GameRules");
                audioSource.clip = FindObjectOfType<GameRules>().audioClipRoadMusic;
            }
            else
            {
                audioSource.clip = audioClip;
            }
            audioSource.playOnAwake = false;

            // Set
            generationMethod.Init(tileSize, audioSource.clip, tileSet_Forest, debug);

            StartCoroutine(GenerateRoad());
        }

        private void Update()
        {
            // Check if audioSource is done playing to add filler audio
            if(GameManager.GameState == 1001 && !audioSource.isPlaying)
            {
                // Game is playing and audiosource is done playing
                audioSource.clip = audioClipFiller;
                audioSource.loop = true;
                audioSource.volume = 0.25f;
                audioSource.Play();
            }
        }

        private IEnumerator GenerateRoad()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            GameManager.RoadGenerationState = 0;

            StartCoroutine(generationMethod.Generate());

            yield return null;
            GameManager.RoadGenerationState = 1;
            stopwatch.Stop();
            UnityEngine.Debug.Log("[Road Generation] Generated Road in: " + stopwatch.Elapsed);
            yield break;
        }

        private void OnDrawGizmos()
        {
            if(Application.isPlaying)
            {
                if(debug.showGrid)
                {
                    GenerationMethodTiles t = generationMethod as GenerationMethodTiles;
                    foreach(Vector2 item in t.grid)
                    {
                        Gizmos.color = Gizmos.color == Color.blue ? Color.red : Color.blue;
                        Gizmos.DrawWireCube(new Vector3(item.x, 0, item.y), Vector3.one);
                    }
                }                
            }
        }

        [System.Serializable]
        public class Debug
        {
            [Tooltip("Show debug messages")]
            public bool show = false;
            [Tooltip("Show the positions of the generated grid")]
            public bool showGrid = false;
            [Tooltip("Generated ownly the road")]
            public bool debugQuickGeneration = false;
            [Tooltip("Combine meshes into a single mesh for optimalization")]
            public bool meshGenerationOptimazation = true;
        }
    }
}