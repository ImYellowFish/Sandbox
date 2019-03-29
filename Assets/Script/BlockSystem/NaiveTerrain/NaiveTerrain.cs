using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveTerrain
{
    public class NaiveTerrain : MonoBehaviour
    {
        public Terrain refTerrain;
        private Terrain terrain;
        private TerrainData data;
        public float cellSize = 1f;

        public int[,] grid;
        public int gridSize;

        public AnimationCurve falloff;

        private void Start()
        {
            terrain = GetComponent<Terrain>();
            data = terrain.terrainData;
            data.SetHeights(0, 0, refTerrain.terrainData.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution));
            terrain.Flush();
            InitGrid();
        }

        private void InitGrid()
        {
            gridSize = Mathf.CeilToInt(data.size.x / cellSize);
            grid = new int[gridSize, gridSize];
            for(int i = 0; i < gridSize; i++)
            {
                for(int j = 0; j < gridSize; j++)
                {
                    var heightmapCoord = GetHeightMapCoord(new Int3(i, 0, j));
                    var heights = data.GetHeights(heightmapCoord.x, heightmapCoord.y, heightmapCoord.z, heightmapCoord.z);
                    var maxHeight = GetMaxHeight(heights);
                    grid[i, j] = Mathf.FloorToInt(maxHeight * data.size.y / cellSize);
                }
            }
        }

        private float GetMaxHeight(float[,] heights)
        {
            float maxHeight = float.MinValue;
            foreach(var height in heights)
            {
                maxHeight = Mathf.Max(height, maxHeight);
            }
            return maxHeight;
        }

        public Vector3 TestPos;
        public float TestHeight;
        [ContextMenu("Test")]
        public void Test()
        {
            var height = data.GetInterpolatedHeight(TestPos.x / data.size.x, TestPos.z / data.size.z);
            TestPos.y = height;
            //Debug.Log(TestPos.y);
            DigAtPos(TestPos);
        }

        public void DigAtPos(Vector3 pos)
        {
            // This method recomputes all the LOD and vegetation 
            // information for the terrain on each call, which can be computationally expensive.
            // In interactive editing scenarios, 
            // it may be better to call TerrainData.SetHeightsDelayLOD instead, 
            // followed by Terrain.ApplyDelayedHeightmapModification when the user 
            // completes an editing action.
            var coord = GetCellCoord(pos);
            coord.y = Mathf.Abs(grid[coord.x, coord.z]);
            Debug.Log(string.Format("Coord {0} to {1}", coord.y, -Mathf.Max(coord.y - 1, 0)));
            grid[coord.x, coord.z] = -Mathf.Max(coord.y - 1, 0);
            var heightMapCoord = GetHeightMapCoord(coord);
            int xbase = heightMapCoord.x;
            int ybase = heightMapCoord.y;
            //Debug.Log(xbase);
            //Debug.Log(ybase);
            int dimension = heightMapCoord.z;
            float heightBase = coord.y * cellSize / data.size.y;
            Debug.Log("heightBase:" + coord.y * cellSize);
            //Debug.Log(data.heightmapHeight);
            var old_heights = data.GetHeights(xbase, ybase, dimension, dimension);
            var heights = new float[dimension, dimension];
            
            for (var i = 0; i < dimension; i++)
            {
                for (var j = 0; j < dimension; j++)
                {
                    float t_x = (float)i / dimension;
                    float t_y = (float)j / dimension;
                    float h0_x = Mathf.Lerp(old_heights[j, 0], old_heights[j, dimension - 1], t_x);
                    float h0_y = Mathf.Lerp(old_heights[0, i], old_heights[dimension - 1, i], t_y);
                    
                    //float tmp = Mathf.Abs(t_x * 2 - 1) + Mathf.Abs(t_y * 2 - 1);
                    //float t_xny = tmp > 0.0001f ? Mathf.Abs(t_x * 2 - 1) / tmp : 0.5f;
                    heights[j, i] = Mathf.Min(h0_x - heightBase, h0_y - heightBase) * (1f - (1f - falloff.Evaluate(t_x)) * (1f - falloff.Evaluate(t_y))) + heightBase;
                    //heights[j, i] = cellSize / data.size.y * (1f - (1f - falloff.Evaluate(t_x)) * (1f - falloff.Evaluate(t_y))) + heightBase;
                    //heights[j, i] = heightBase;
                }
            }
            data.SetHeights(xbase, ybase, heights);
            CheckNeighbor(coord);
            terrain.Flush();
        }

        public void CheckNeighbor(Int3 coord)
        {
            if(ShouldBlendNeighbor(coord, coord.Add(-1, 0, 0))){
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y, dimension, dimension);
                var heights = new float[dimension, dimension];
                float heightBase = coord.y * cellSize / data.size.y;
                for (int i = 0; i < dimension; i++)
                {
                    for(int j = 0; j < dimension; j++)
                    {
                        float t_y = (float)j / dimension;
                        float h0_y = Mathf.Lerp(old_heights[0, i], old_heights[dimension - 1, i], t_y);
                        heights[j, i] = Mathf.Min(old_heights[j, i], (h0_y - heightBase) * falloff.Evaluate(t_y) + heightBase);
                    }
                }
                data.SetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y, heights);
            }
            if (ShouldBlendNeighbor(coord, coord.Add(1, 0, 0)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y, dimension, dimension);
                var heights = new float[dimension, dimension];
                float heightBase = coord.y * cellSize / data.size.y;
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        float t_y = (float)j / dimension;
                        float h0_y = Mathf.Lerp(old_heights[0, i], old_heights[dimension - 1, i], t_y);
                        heights[j, i] = Mathf.Min(old_heights[j, i], (h0_y - heightBase) * falloff.Evaluate(t_y) + heightBase);
                    }
                }
                data.SetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y, heights);
            }
            if (ShouldBlendNeighbor(coord, coord.Add(0, 0, -1)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x, heightMapCoord.y - dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                float heightBase = coord.y * cellSize / data.size.y;
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        float t_x = (float)i / dimension;
                        float h0_x = Mathf.Lerp(old_heights[j, 0], old_heights[j, dimension - 1], t_x);
                        heights[j, i] = Mathf.Min(old_heights[j,i], (h0_x - heightBase) * falloff.Evaluate(t_x) + heightBase);
                    }
                }
                data.SetHeights(heightMapCoord.x, heightMapCoord.y - dimension / 2, heights);
            }
            if (ShouldBlendNeighbor(coord, coord.Add(0, 0, 1)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x, heightMapCoord.y + dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                float heightBase = coord.y * cellSize / data.size.y;
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        float t_x = (float)i / dimension;
                        float h0_x = Mathf.Lerp(old_heights[j, 0], old_heights[j, dimension - 1], t_x);
                        heights[j, i] = Mathf.Min(old_heights[j, i], (h0_x - heightBase) * falloff.Evaluate(t_x) + heightBase);
                    }
                }
                data.SetHeights(heightMapCoord.x, heightMapCoord.y + dimension / 2, heights);
            }

            // diagonal
            if (ShouldBlendNeighbor(coord, coord.Add(-1, 0, 0)) &&
                ShouldBlendNeighbor(coord, coord.Add(0, 0, -1))){
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y - dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        heights[j, i] = Mathf.Min(coord.y * cellSize / data.size.y, old_heights[j, i]);
                    }
                }
                data.SetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y - dimension / 2, heights);
            }

            
            if (ShouldBlendNeighbor(coord, coord.Add(-1, 0, 0)) &&
                ShouldBlendNeighbor(coord, coord.Add(0, 0, 1)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y + dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        heights[j, i] = Mathf.Min(coord.y * cellSize / data.size.y, old_heights[j, i]);
                    }
                }
                data.SetHeights(heightMapCoord.x - dimension / 2, heightMapCoord.y + dimension / 2, heights);
            }

            if (ShouldBlendNeighbor(coord, coord.Add(1, 0, 0)) &&
                ShouldBlendNeighbor(coord, coord.Add(0, 0, -1)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y - dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        heights[j, i] = Mathf.Min(coord.y * cellSize / data.size.y, old_heights[j, i]);
                    }
                }
                data.SetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y - dimension / 2, heights);
            }

            if (ShouldBlendNeighbor(coord, coord.Add(1, 0, 0)) &&
                ShouldBlendNeighbor(coord, coord.Add(0, 0, 1)))
            {
                var heightMapCoord = GetHeightMapCoord(coord);
                var dimension = heightMapCoord.z;
                var old_heights = data.GetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y + dimension / 2, dimension, dimension);
                var heights = new float[dimension, dimension];
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        heights[j, i] = Mathf.Min(coord.y * cellSize / data.size.y, old_heights[j, i]);
                    }
                }
                data.SetHeights(heightMapCoord.x + dimension / 2, heightMapCoord.y + dimension / 2, heights);
            }

        }

        public bool IsInGrid(Int3 coord)
        {
            return coord.x >= 0 && coord.x < gridSize && coord.z >= 0 && coord.z < gridSize;
        }

        public bool ShouldBlendNeighbor(Int3 coord0, Int3 coord1)
        {
            Debug.Log("ShouldBlendNeighbor, " + grid[coord1.x, coord1.z] + ", " + grid[coord0.x, coord0.z]);
            if (IsInGrid(coord1))
            {
                if((grid[coord1.x, coord1.z] <= 0) && Mathf.Abs(grid[coord1.x, coord1.z]) <= Mathf.Abs(grid[coord0.x, coord0.z]))
                {
                    Debug.Log("Check pass.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }  
        }

        public Int3 GetHeightMapCoord(Int3 cellCoord)
        {
            int pixelPerUnit = (int)(cellSize / data.size.x * data.heightmapResolution);
            Int3 heightMapCoord;
            heightMapCoord.x = cellCoord.x * pixelPerUnit;
            heightMapCoord.y = cellCoord.z * pixelPerUnit;
            heightMapCoord.z = pixelPerUnit;
            return heightMapCoord;
        }

        public void UpdateHeightAtCoord(Int3 coord)
        {
            int pixelPerUnit = (int)(cellSize / data.size.x * data.heightmapResolution);
            int xbase = coord.x * pixelPerUnit;
            int ybase = coord.z * pixelPerUnit;
            int dimension = pixelPerUnit;
            float heightBase = coord.y * cellSize / data.size.y;
            //Debug.Log(data.heightmapHeight);
            var old_heights = data.GetHeights(xbase, ybase, dimension, dimension);
            var heights = new float[dimension, dimension];
            for (var i = 0; i < dimension; i++)
            {
                for (var j = 0; j < dimension; j++)
                {
                    heights[j, i] = Mathf.Lerp(heightBase, old_heights[j, i], GetWeight(i, j, dimension));
                }
            }
            data.SetHeights(xbase, ybase, heights);
        }

        public float GetWeight(int i, int j, int dimension)
        {
            float x = (float)i / dimension - 0.5f;
            float y = (float)j / dimension - 0.5f;
            float t = Mathf.Max(x * x, y * y);
            return t * 4f;
        }

        public Int3 GetCellCoord(Vector3 pos)
        {
            return new Int3
            {
                x = (int)(pos.x / cellSize),
                y = (int)(pos.y / cellSize),
                z = (int)(pos.z / cellSize),
            };
        }

        public int GetCellCoord(float v)
        {
            return (int)(v / cellSize);
        }



        public struct Int3
        {
            public int x;
            public int y;
            public int z;

            public Int3(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public override string ToString()
            {
                return string.Format("int3({0},{1},{2})", x, y, z);
            }

            public Int3 Add(int x, int y, int z)
            {
                return new Int3(this.x + x, this.y + y, this.z + z);
            }
        }
    }
}