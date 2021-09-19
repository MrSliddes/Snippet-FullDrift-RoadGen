using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FD.Generation
{
    public class GenerationMethodTiles : GenerationMethod
    {
        /// <summary>
        /// The grid of the road generation
        /// </summary>
        public HashSet<Vector2> grid = new HashSet<Vector2>(); // Public for debugging purpose

        /// <summary>
        /// The direction road is currently generated in. 0 = forward, -1 = left, 1 = right
        /// </summary>
        private int currentGenerationDirection = 0;
        /// <summary>
        /// The current sampleValue from 0 to 1
        /// </summary>
        private float currentSampleValue;
        /// <summary>
        /// The total amount of seconds generated with road
        /// </summary>
        private float generatedSecondsTotal;
        /// <summary>
        /// The equal value difference between each tileset group. Dont care about -1 to 1, its 0 to 1
        /// </summary>
        private float tileGroupsSampleDifference;
        /// <summary>
        /// The current position where a new road gets generated
        /// </summary>
        private Vector2 currentGenerationPosition = Vector2.zero;
        /// <summary>
        /// Contains all road gameobjects that are instantiated
        /// </summary>
        private Transform parent;


        /// <summary>
        /// Generate a road with the method tiles
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="tileSet"></param>
        /// <returns>Nothing</returns>
        public override IEnumerator Generate()
        {
            // Convert audioClip to usefull data
            float[] samples = new float[audioClip.samples];
            if(debug.show) Debug.Log("[GMTiles] Samples from audioClip: " + samples.Length);
            // Fill samples array with audioClip data
            audioClip.GetData(samples, 0);

            // Short the samples array to have the length equal to the amount of seconds of the audioClip
            MathC.ShortArray(ref samples, Mathf.RoundToInt(audioClip.length));
            // Smooth the array to have bigger values
            MathC.EnlargeArrayValues(ref samples, -1, 1);
            if(debug.show) Debug.Log("[GMTiles] Samples after short: " + samples.Length);

            // We now have an array with values from -1 to 1
            // We need to check the change rate from index to index
            // If the change is high that means the music is changing fast

            // Split array into group sections
            // A group needs a min amount of seconds + change rate has to be x higher than previous for new group
            float groupMinAmountOfSeconds = 10;
            float minChangeRate = 0.4f; // or get average change rate by comparing all array values?

            // Create a group list that contains the the group info
            List<Group> groups = new List<Group>();
            groups.Add(new Group());

            // Loop trough samples and group them
            for(int i = 0; i < samples.Length; i++)
            {
                // Check if current group needs more values
                if(groups[groups.Count - 1].samplesIndexes.Count < groupMinAmountOfSeconds)
                {
                    // Add sample value to current group
                    groups[groups.Count - 1].samplesIndexes.Add(i);
                }
                else
                {
                    // Check if we want to add the sample index to the group or if the change rate is too high create a new group
                    // Compare current index value with previous index value
                    if(MathC.ValueDiff(samples[i], samples[i - 1]) >= minChangeRate)
                    {
                        // Create a new group
                        groups.Add(new Group());
                        // Add index
                        groups[groups.Count - 1].samplesIndexes.Add(i);
                    }
                    else
                    {
                        // Value diff is too small so add it to current group
                        groups[groups.Count - 1].samplesIndexes.Add(i);
                    }
                }
            }  

            // For each group get the sample average
            float heighestSample = 0;
            for(int i = 0; i < groups.Count; i++)
            {
                groups[i].GetSamplesIndexesAverage(ref samples);
                // Get heighest sample value
                if(Mathf.Abs(groups[i].sampleAverage) > heighestSample) heighestSample = groups[i].sampleAverage;
            }

            // Set seed randomness based on total sample value
            Random.InitState((int)heighestSample);

            // Calculate tileGroupsSampleDifference.
            tileGroupsSampleDifference = heighestSample / tileSet.groups.Length;
            //Debug.LogWarning(tileGroupsSampleDifference);

            // We now have the average sample value per group and the seconds amount of the group
            // For each group we need to fill the seconds with drivable road
            // The type of road that is generated is determend by the group sampleAverage
            // The tiles are sorted into small, medium & large.
            // Based on groups sampleAverage the group falls into a category of tileSet.groups
            // In that group it picks a random tile based on tile spawnChance

            // Create parent transform for road gameobjects
            parent = new GameObject("Road Generation Parent").transform;
            // Add road at 0,0
            AddRoadAt(new Vector2(0, 0), tileSet.groups[0].tiles[0]);

            // Loop trough groups and fill in the seconds with tiles
            for(int i = 0; i < groups.Count; i++)
            {
                currentSampleValue = Mathf.Abs(groups[i].sampleAverage);
                                                
                // Fill in the groups seconds
                float secondsLeft = groups[i].Seconds;
                while(secondsLeft > 0)
                {
                    // Get a road to generate
                    AddRoadAt(currentGenerationPosition, GetRoadTile(ref secondsLeft));

                    if(generatedSecondsTotal >= audioClip.length) break;
                }

                if(generatedSecondsTotal >= audioClip.length) break;
            }


            // After generating all road tiles add start and finish
            // Finish is after last tile
            // Check if currentGenerationDirection ends at 0 for finish (forward) else generate it to 0
            AddRoadAt(currentGenerationPosition, tileSet.tileFinish);
            
            // Start is behind 0,0
            Object.Instantiate(tileSet.tileStart.prefab, new Vector3(0, 0, -tileSize.y), Quaternion.identity, parent);
            grid.Add(new Vector2(0, -tileSize.y * 0.5f));
            

            // Fill in the tiles surrounding with filler tile
            if(!debug.debugQuickGeneration)
            {
                int margin = 2; // The tile thickness generated around the grid corners
                // Loop trough grid, and get corners for filling in
                Vector2 cornerBottomLeft, cornerTopRight = Vector2.zero;
                cornerBottomLeft.x = grid.Min(x => x.x);
                cornerBottomLeft.y = -margin * tileSize.y;
                cornerTopRight.x = grid.Max(x => x.x) + margin * tileSize.x;
                cornerTopRight.y = grid.Max(x => x.y) + margin * tileSize.y;
                if(debug.show) if(debug.show) Debug.Log("[Road Generation] cornerBottomLeft: " + cornerBottomLeft + " cornerTopRight: " + cornerTopRight);

                Material m = Resources.Load<Material>("Mat_ForestTreeGreen");

                for(int x = (int)cornerBottomLeft.x; x < cornerTopRight.x; x += (int)(tileSize.x))
                {
                    for(int z = (int)cornerBottomLeft.y; z < cornerTopRight.y; z += (int)(tileSize.y))
                    {
                        // Check if pos isnt already occupied by grid
                        if(!grid.Contains(new Vector2(x, z + tileSize.y * 0.5f)))
                        {
                            // Create filler tile
                            GameObject a = Object.Instantiate(tileSet.tileFiller, Vector3.zero, Quaternion.identity, parent);

                            // Combine all filler tile meshes to 1 for optimazation
                            if(debug.meshGenerationOptimazation)
                            {
                                MeshFilter[] meshFilters = a.GetComponentsInChildren<MeshFilter>();
                                CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                                int i = 0;
                                while(i < meshFilters.Length)
                                {
                                    combine[i].mesh = meshFilters[i].sharedMesh;
                                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                                    meshFilters[i].gameObject.SetActive(false);

                                    i++;
                                }
                                a.gameObject.AddComponent<MeshFilter>();
                                a.gameObject.AddComponent<MeshRenderer>().material = m;

                                a.GetComponent<MeshFilter>().mesh = new Mesh();
                                a.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
                                a.GetComponent<MeshFilter>().mesh.Optimize();
                                a.gameObject.SetActive(true);
                            }
                            a.transform.position = new Vector3(x, 0, z);
                        }
                    }
                }

            }

            Debug.Log("[Road Generation] Generated road worth: " + generatedSecondsTotal + " seconds. Song length is: " + audioClip.length + " seconds");
            yield break;
        }

        /// <summary>
        /// Add a road prefab at a given position
        /// </summary>
        /// <param name="pos">The x and y (z) of the tile pivot position</param>
        /// <param name="tile">The tile to create</param>
        /// <param name="checkForError">Do we want to check if the tile fits/correct it or do we just wanna cram it in knowing that it should fit</param>
        private void AddRoadAt(Vector2 pos, Tile tile, bool checkForError = true)
        {
            // We got the tile we want to spawn, but we need to check
            // 1. The rotation of the tile
            // 2. If the tile fits in the grid or if we need to make more room

            // Create temp tile with correct values
            if(debug.show) Debug.Log(tile);
            Tile correctTile = new Tile(tile);
            // Rotation of prefab instatiated
            Quaternion rot = Quaternion.Euler(0, 0, 0);
                        
            // Check if we need to place an adjustment prefab before the actual tile
            if(tile.startDirection != 0 && currentGenerationDirection == 0 && checkForError)
            {
                if(debug.show) if(debug.show) Debug.Log("Made adjustment at: " + pos);
                // Place prefabAdjustment
                Tile adjustmentTile = new Tile(tileSet.groups[tile.adjustmentTileIndex.x].tiles[tile.adjustmentTileIndex.y]); // If outOfIndex, its grabbing a -1,-1 index which doesnt exists

                // Check if adjustment tile fits
                if(debug.show) Debug.Log("before1: " + pos);
                CheckIfTileFits(ref pos, adjustmentTile);
                if(debug.show) Debug.Log("after2: " + pos);

                // add to grid
                for(int i = 0; i < adjustmentTile.shape.Length; i++)
                {
                    grid.Add(pos + adjustmentTile.shape[i] * tileSize);
                }

                // Instantiate
                Object.Instantiate(adjustmentTile.prefab, new Vector3(pos.x, 0, pos.y), rot, parent);

                // Update pos
                pos += new Vector2(adjustmentTile.endPosition.x * tileSize.x, adjustmentTile.endPosition.y * tileSize.y);
                currentGenerationPosition = pos;
            }

            // Rotate tile if needed to currentGenerationDirection
            if(currentGenerationDirection == 0 || tile.startDirection == currentGenerationDirection)
            {
                // Dont have to change anything
            }
            else if(currentGenerationDirection == -1)
            {
                rot = Quaternion.Euler(0, -90, 0);

                // Direction is to the left, so tiles have to be rotated to the left
                // If the rotated tile endPosition is also to the left grab the mirror variant so it faces right (or globaly up)
                if(tile.endDirection == 0)
                {
                    correctTile.endDirection = -1;
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 3);
                    correctTile.endPosition = MathC.RotateVector2Visually(tile.endPosition, 3);

                    // Check if endPosition is below currentGenerationPosition
                    if(IsEndPositionNegative(correctTile.endPosition))
                    {
                        // Use prefabMirror and flip stuff correctly
                        correctTile.prefab = tile.prefabMirror;
                        correctTile.shape = MathC.FlipVector2Visually(correctTile.shape, false);
                        correctTile.endPosition = MathC.FlipVector2Visually(correctTile.endPosition, false);
                    }
                }
                else if(tile.endDirection == -1)
                {
                    // If this tile rotates to the left it would face down and thats not allowed
                    // So we grab the mirror tile which result in the direction facing up
                    correctTile.prefab = tile.prefabMirror;
                    
                    // Rotate to left, then flip
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 3);
                    correctTile.shape = MathC.FlipVector2Visually(correctTile.shape, false);

                    // Rotate to left, then flip
                    correctTile.endPosition = MathC.RotateVector2Visually(tile.endPosition, 3);
                    correctTile.endPosition = MathC.FlipVector2Visually(correctTile.endPosition, false);
                    correctTile.endDirection = 0;
                }
                else if(tile.endDirection == 1)
                {
                    correctTile.endDirection = 0;
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 3);
                    correctTile.endPosition = MathC.RotateVector2Visually(tile.endPosition, 3);
                }
            }
            else if(currentGenerationDirection == 1)
            {
                rot = Quaternion.Euler(0, 90, 0);

                // Direction is to the right, so tiles have to rotated to the right
                // Tiles have default left variant so we dont need to grab a mirror variant
                if(tile.endDirection == 0)
                {
                    correctTile.endDirection = 1;
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 1);
                    correctTile.endPosition = MathC.RotateVector2Visually(tile.endPosition, 1);
                    if(debug.show) Debug.Log(correctTile.endPosition);
                    // Check if endPosition is below currentGenerationPosition
                    if(IsEndPositionNegative(correctTile.endPosition))
                    {
                        // Use prefabMirror and flip stuff correctly
                        correctTile.prefab = tile.prefabMirror;
                        correctTile.shape = MathC.FlipVector2Visually(correctTile.shape, false);
                        correctTile.endPosition = MathC.FlipVector2Visually(correctTile.endPosition, false);
                        Debug.LogWarning("isNegativ");
                    }
                }
                else if(tile.endDirection == -1)
                {
                    correctTile.endDirection = 0;
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 1);
                    correctTile.endPosition = MathC.RotateVector2Visually(new Vector2[1] { tile.endPosition }, 1)[0];
                }
                else if(tile.endDirection == 1)
                {
                    // If this tile rotates to the right it would face down and thats not allowed
                    // So we grab the mirror tile which result in the direction facing up
                    correctTile.prefab = tile.prefabMirror;

                    // Rotate to right, then flip
                    correctTile.shape = MathC.RotateVector2Visually(tile.shape, 1);
                    correctTile.shape = MathC.FlipVector2Visually(correctTile.shape, false);

                    // Rotate to right, then flip
                    correctTile.endPosition = MathC.RotateVector2Visually(tile.endPosition, 1);
                    correctTile.endPosition = MathC.FlipVector2Visually(correctTile.endPosition, false);

                    correctTile.endDirection = 0;
                }
            }

            // Check for overlap
            if(checkForError) CheckIfTileFits(ref pos, correctTile);

            if(debug.show) Debug.Log(correctTile.prefab);            
            // Add the correct grid positions of the tile
            for(int i = 0; i < correctTile.shape.Length; i++)
            {
                grid.Add(pos + correctTile.shape[i] * tileSize);
                if(debug.show) Debug.Log("GridPos: " + correctTile.shape[i] * tileSize);
            }

            Object.Instantiate(correctTile.prefab, new Vector3(pos.x, 0, pos.y), rot, parent);

            // Get new generation direction
            currentGenerationDirection = correctTile.endDirection;
            currentGenerationPosition += new Vector2(correctTile.endPosition.x * tileSize.x, correctTile.endPosition.y * tileSize.y);
            if(debug.show) Debug.Log("Current Gen Pos: " + currentGenerationPosition);
            // Update secondsTotal
            generatedSecondsTotal += tile.secondsToComplete;
        }

        /// <summary>
        /// Get a road prefab based on sampleValue (-1 to 1) and seconds it can fill
        /// </summary>
        /// <param name="sampleValue">-1, to 1 value of sample</param>
        /// <param name="secondsLeft">how much seconds the road would need to fill</param>
        /// <returns>A tile fitting to sampleValue and secondsLeft</returns>
        private Tile GetRoadTile(ref float secondsLeft)
        {
            Tile tile = new Tile();
            // From current sample value get a tileset group
            for(int i = 0; i < tileSet.groups.Length; i++)
            {
                // Check if currentSampleValue is in group sample value range. Example: 0 - 0.3, 0.3 - 0.6, 0.6 - 0.9 or last one
                if(currentSampleValue <= (i + 1) * tileGroupsSampleDifference || i == tileSet.groups.Length - 1)
                {
                    // Correct group, pick a tile randomly with tile spawnWeight
                    int totalWeight = tileSet.groups[i].tiles.Sum(x => x.spawnWeight);
                    int r = UnityEngine.Random.Range(0, totalWeight + 1);
                    for(int j = 0; j < tileSet.groups[i].tiles.Length; j++)
                    {                        
                        // Check if r is lower or equal to that of spawn chance, and if it is we got our tile, but we gotta check for rotation
                        if(tileSet.groups[i].tiles[j].spawnWeight <= Mathf.Abs(r) || j == tileSet.groups[i].tiles.Length - 1)
                        {
                            // Also check if tile can fit into current generation direction (cant have 2 left u-shape tiles after eachother)
                            //if(debug.show) Debug.Log(tileSet.groups[i].tiles[j].startDirection == 0);
                            //if(debug.show) Debug.Log(tileSet.groups[i].tiles[j].startDirection == currentGenerationDirection);
                            if(tileSet.groups[i].tiles[j].startDirection == 0 || tileSet.groups[i].tiles[j].startDirection == currentGenerationDirection || currentGenerationDirection == 0)
                            {
                                tile = tileSet.groups[i].tiles[j];
                                if(debug.show) Debug.Log("Picked tile: " + tile.prefab);
                                break;
                            }
                        }
                        // Not matching, reduce spawnWeight
                        r -= tileSet.groups[i].tiles[j].spawnWeight;
                    }                    
                    // break out of loop
                    break;
                }
                // Next group
            }

            if(tile.prefab == null)
            {
                Debug.LogWarning("EMPTY ROAD GET");
                tile = tileSet.groups[0].tiles[0];
            }

            // We got the tile to spawn
            secondsLeft -= tile.secondsToComplete;            
            return tile;
        }

        /// <summary>
        /// Check if a tile endposition is negative (meaning lower than current position)
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>True if tile endPosition is lower than currentPosition</returns>
        private bool IsEndPositionNegative(Vector2 pos)
        {
            return currentGenerationPosition.y + pos.y < currentGenerationPosition.y;
        }

        /// <summary>
        /// Checks if the tile overlaps in existing grid
        /// </summary>
        /// <returns>True if it overlaps, false if it doesnt</returns>
        private bool TileOverlapsGrid(Vector2 pos, Tile tile)
        {
            // Check Tile endPosition next tile
            // Because tile.EndPosition stops at the edge of the tile and not in the tile next to it we need to add correction (check next tile position instead on edge)
            Vector2 endPositionCorrection = Vector2.zero;
            if(tile.endDirection == 0) endPositionCorrection.y += 0.5f;
            else if(tile.endDirection == -1) endPositionCorrection.x -= 0.5f;
            else if(tile.endDirection == 1) endPositionCorrection.x += 0.5f;

            bool overlaps = grid.Contains(pos + (tile.endPosition + endPositionCorrection) * tileSize);
            if(debug.show) Debug.Log("no overlap endpos at: " + (pos + tile.endPosition * tileSize));
            if(overlaps) if(debug.show) Debug.Log("Overlap at: " + (pos + tile.endPosition * tileSize));

            // Check shape
            for(int i = 0; i < tile.shape.Length; i++)
            {
                if(grid.Contains(pos + tile.shape[i] * tileSize))
                {
                    if(debug.show) Debug.Log("Overlap at: " + (pos + tile.shape[i] * tileSize));
                    overlaps = true;
                }
                if(overlaps) break;
            }

            return overlaps;
        }

        /// <summary>
        /// Check if a tile fits, and if it doesnt make it fit
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tile"></param>
        private void CheckIfTileFits(ref Vector2 pos, Tile tile)
        {
            if(TileOverlapsGrid(pos, tile))
            {
                int infinitLoopPrevention = 20;
                bool needAfterCurve = true; // If currentGenerationDirection != 0 we need to add a curve back to the dir to prevent a TileOverlapsGrid bug (certain tiles would have wrong offset that doesnt trigger tile overlapsgrid)
                int prevCurrentGenerationDirection = currentGenerationDirection;

                // Push piece forward till it fits
                // First add a tile that makes the currentGenerationDirection go foward
                Tile t = null;
                if(currentGenerationDirection == 0) 
                {
                    // Straight road are generated in the while loop
                    needAfterCurve = false;
                }
                else if(currentGenerationDirection == -1)
                {
                    // Generate a curve right
                    if(debug.show) Debug.Log("Gen Curve Right");
                    t = new Tile(tileSet.groups[0].tiles[2]);
                    AddRoadAt(pos, t, false);
                }
                else if(currentGenerationDirection == 1)
                {
                    // Generate a curve left
                    if(debug.show) Debug.Log("Gen Curve left");
                    t = new Tile(tileSet.groups[0].tiles[1]);
                    AddRoadAt(pos, t, false);
                }

                // CurrentGenerationDir should be 0 after above
                // Update pos since it doesnt get updated at AddRoadAt (the ref pos doesnt get updated when we use AddRoadAt())
                if(t != null) pos = currentGenerationPosition;

                if(debug.show) Debug.Log("after gen");

                // Gameobject for if a curve gets placed after straight road
                GameObject afterCurve = null;
                Tile afterCurveTile = null;
                // Used to reset pos if tile overlaps with afterCurve
                Vector2 prevPos = Vector2.zero;
                
                // Check if correctTile fits, if it doesnt keep adding straight road pieces till it does
                while(true)
                {
                    // Does correctTile fit?
                    if(TileOverlapsGrid(pos, tile) && needAfterCurve == false || needAfterCurve && afterCurve == null && TileOverlapsGrid(pos, tile) || needAfterCurve && afterCurve == null)
                    {
                        // Did we add curve previous loop?
                        if(afterCurve != null)
                        {
                            // We did, remove curve, remove shape, reset pos and reset loop
                            Object.Destroy(afterCurve);
                            for(int i = 0; i < afterCurveTile.shape.Length; i++)
                            {
                                grid.Remove(prevPos + afterCurveTile.shape[i] * tileSize);
                            }
                            pos = prevPos;
                            afterCurve = null;
                            continue;
                        }

                        // Add straight road and check next loop
                        Tile straight = new Tile(tileSet.groups[0].tiles[0]);
                        // Add to grid
                        for(int i = 0; i < straight.shape.Length; i++)
                        {
                            grid.Add(pos + straight.shape[i] * tileSize);
                        }
                        // Instantiate
                        Object.Instantiate(straight.prefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity, parent);
                        // Update pos
                        pos += new Vector2(straight.endPosition.x * tileSize.x, straight.endPosition.y * tileSize.y);
                        prevPos = pos; // Backup pos if curve isnt good, this has to happen after adding the straight pos (if curve isnt good, it gets removed and pos gets resetted to prevPo, so the last straight tile pos)s
                        // Add tile to total seconds
                        generatedSecondsTotal += tile.secondsToComplete;

                        // If generation was horizontal (-1, 1) add correct curve back to match prev direction
                        if(needAfterCurve)
                        {
                            // Add curve, then in next loop it check if it doesnt overlap.
                            // If it does, remove curve object, reset pos before curve placement, remove shape from grid if curve was placed
                            afterCurveTile = prevCurrentGenerationDirection == -1 ? new Tile(tileSet.groups[0].tiles[1]) : new Tile(tileSet.groups[0].tiles[2]);

                            // Add to grid (dont need to check if it fits since is being generated forward)
                            for(int i = 0; i < afterCurveTile.shape.Length; i++)
                            {
                                grid.Add(pos + afterCurveTile.shape[i] * tileSize);
                            }

                            // Instantiate
                            afterCurve = Object.Instantiate(afterCurveTile.prefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity, parent);

                            // Update pos
                            pos += new Vector2(afterCurveTile.endPosition.x * tileSize.x, afterCurveTile.endPosition.y * tileSize.y);
                        }
                    }
                    else
                    {
                        // Tile fits
                        // Update current generation position
                        currentGenerationPosition = pos;
                        // If needAfterCurve add the seconds of the curve tile
                        if(needAfterCurve) generatedSecondsTotal += afterCurveTile.secondsToComplete;
                        if(debug.show) Debug.Log("tile fits, last pos: " + pos);
                        break;
                    }

                    infinitLoopPrevention--;
                    if(infinitLoopPrevention <= 0)
                    {
                        Debug.LogError("Check to many times for overlap");
                        break;
                    }
                }
            }
            else if(debug.show) Debug.Log("tile doesnt overlap at: " + pos);
        }

        /// <summary>
        /// A sample group countaining sample values that belong togeter. Not to be confused with TileGroup
        /// </summary>
        private class Group
        {
            /// <summary>
            /// Get the amount of seconds a group is worth
            /// </summary>
            public int Seconds { get { return samplesIndexes.Count; } }

            /// <summary>
            /// Contains the indexes from samples that belong to this group
            /// </summary>
            public List<int> samplesIndexes = new List<int>();
            /// <summary>
            /// The sample average value from all sample indexes that belong to this group
            /// </summary>
            public float sampleAverage;

            /// <summary>
            /// Get the sample average value from all samplesIndexes
            /// </summary>
            public void GetSamplesIndexesAverage(ref float[] samples)
            {
                // Loop trough samplesIndexes, count all values up, and then devide by total values to get average
                float total = 0;
                for(int i = 0; i < samplesIndexes.Count; i++)
                {
                    total += samples[samplesIndexes[i]];
                }
                sampleAverage = total / (float)samplesIndexes.Count;
            }
        }
    }    
}