using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingSquare {
    /// <summary>
    /// Contains a group of cells
    /// Pivot point is at bottom-left
    /// </summary>
    [System.Serializable]
    public class Trunk
    {
        /// <summary>
        /// Number of cells per row/column
        /// Auto converted to closest power of 2
        /// </summary>
        public int cellsPerRow;

        /// <summary>
        /// log2(cellsPerRow)
        /// </summary>
        public int cellsPerRowPowerOfTwo;

        /// <summary>
        /// Size of each cell
        /// </summary>
        public float cellSize;

        /// <summary>
        /// Inverse size
        /// </summary>
        public float inverseCellSize;

        /// <summary>
        /// Stores the cell vertex value
        /// </summary>
        public int[,] grid;

        public int maxCellValue = 256;
        public int maxCellValuePowerOfTwo;

        private Dictionary<int, int> indexLookup = new Dictionary<int, int>();
        private List<Vector3> vertices = new List<Vector3>(100);
        private List<int> triangles = new List<int>(100);
        
        public Trunk()
        {

        }

        public Trunk(int cellsPerRow, float cellSize)
        {
            this.cellsPerRow = Mathf.ClosestPowerOfTwo(cellsPerRow);
            this.cellsPerRowPowerOfTwo = Mathf.RoundToInt(Mathf.Log(this.cellsPerRow, 2));
            this.maxCellValue = Mathf.ClosestPowerOfTwo(this.maxCellValue);
            this.maxCellValuePowerOfTwo = Mathf.RoundToInt(Mathf.Log(this.maxCellValue, 2));
            this.cellSize = cellSize;
            this.inverseCellSize = 1f / Mathf.Max(cellSize, Mathf.Epsilon);
            this.grid = new int[cellsPerRow + 1, cellsPerRow + 1];
        }
        
        public void GetCellCoordAtPos(Vector2 pos, out int coord_x, out int coord_y)
        {
            coord_x = Mathf.Min((int)(pos.x * inverseCellSize), cellsPerRow - 1);
            coord_y = Mathf.Min((int)(pos.y * inverseCellSize), cellsPerRow - 1);
        }

        public void GetGridIntCoordAtPos(Vector2 pos, out int coord_x, out int coord_y)
        {
            coord_x = Mathf.RoundToInt(pos.x * inverseCellSize);
            coord_y = Mathf.RoundToInt(pos.y * inverseCellSize);
        }

        public Vector2 GetGridCoordAtPos(Vector2 pos)
        {
            return new Vector2(pos.x * inverseCellSize, pos.y * inverseCellSize);
        }

        public void InitGridValue(float value)
        {
            int v = (int)(value * maxCellValue);
            for(int i = 0; i <= cellsPerRow; i++)
            {
                for(int j = 0; j <= cellsPerRow; j++)
                {
                    grid[i, j] = v;
                }
            }
        }

        public float GetValueAtLocalPos(Vector2 pos)
        {
            int coord_x, coord_y;
            GetGridIntCoordAtPos(pos, out coord_x, out coord_y);
            return grid[coord_x, coord_y] / (float)maxCellValue;
        }

        public void SetValueAtPos(Vector2 pos, float value)
        {
            int coord_x, coord_y;
            GetGridIntCoordAtPos(pos, out coord_x, out coord_y);
            SetValueAtCoord(coord_x, coord_y, value); 
        }

        public void SetValueAtCoord(int coord_x, int coord_y, float value){
        	if(coord_x >= 0 && coord_x <= cellsPerRow &&
        		coord_y >= 0 && coord_y <= cellsPerRow){

        		grid[coord_x, coord_y] = (int)(Mathf.Clamp01(value) * maxCellValue);
        	}
        }

        public void AddValueAtCoord(int coord_x, int coord_y, float value){
        	if(coord_x >= 0 && coord_x <= cellsPerRow &&
        		coord_y >= 0 && coord_y <= cellsPerRow){
        		
        		grid[coord_x, coord_y] += (int)(value * maxCellValue);
        		grid[coord_x, coord_y] = Mathf.Clamp(grid[coord_x, coord_y], 0, maxCellValue);
        	}
        }

        public void RecalculateTrunkMesh(Mesh mesh)
        {
            indexLookup.Clear();
            vertices.Clear();
            triangles.Clear();
            int currentVertIndex = 0;
            int thresholdShift = maxCellValuePowerOfTwo - 1;

            for(int i = 0; i < cellsPerRow; i++)
            {
                for (int j = 0; j < cellsPerRow; j++)
                {
                    int[] cellVerts = new int[4]
                    {
                        // bottom left
                        grid[i, j],
                        // bottom right
                        grid[i+1, j],
                        // top right
                        grid[i+1, j+1],
                        // top left
                        grid[i, j+1],
                    };

                    int caseIndex = (cellVerts[0] >> thresholdShift & 1) + 
                        ((cellVerts[1] >> thresholdShift & 1) << 1) + 
                        ((cellVerts[2] >> thresholdShift & 1) << 2) + 
                        ((cellVerts[3] >> thresholdShift & 1) << 3);

                    // Debug.Log("caseIndex:" + caseIndex);

                    var vertsRaw = MarchingSquareData.vertices[caseIndex];
                    var trigRaw = MarchingSquareData.triangles[caseIndex];

                    for(int vi = 0; vi < vertsRaw.Length; vi += 2)
                    {
                        var local_x = vertsRaw[vi];
                        var local_y = vertsRaw[vi + 1];
                        var vert_key = GetCellVertKey(local_x, local_y, i, j);
                        if (!indexLookup.ContainsKey(vert_key))
                        {
                            // New vertex; add it to the vertex list
                            indexLookup[vert_key] = currentVertIndex;
                            currentVertIndex++;

                            //Debug.Log(new Vector4(local_x, local_y, i, j));
                            //Debug.Log(vert_key);
                            //Debug.Log(currentVertIndex);

                            var anchors = MarchingSquareData.anchors[caseIndex][vi / 2];
                            if (anchors == null)
                            {
                                var vertPos = GetCellVertPos(local_x, local_y, i, j, cellSize);
                                vertices.Add(vertPos);
                            }
                            else
                            {
                                float t = (float)cellVerts[anchors[1]] / maxCellValue / 2;
                                var pos_a = MarchingSquareData.cellVertPos[anchors[0]];
                                var pos_b = MarchingSquareData.cellVertPos[anchors[1]];
                                float lerped_x = Mathf.Lerp(pos_a[0], pos_b[0], t);
                                float lerped_y = Mathf.Lerp(pos_a[1], pos_b[1], t);
                                var vertPos = GetCellVertPos(lerped_x, lerped_y, i, j, cellSize);
                                vertices.Add(vertPos);
                            }   
                        }
                    }

                    //Debug.Log("triangles:");
                    foreach(var ti in trigRaw)
                    {
                        var vert_key = GetCellVertKey(vertsRaw[ti * 2], vertsRaw[ti * 2 + 1], i, j);
                        triangles.Add(indexLookup[vert_key]);
                        //Debug.Log(indexLookup[vert_key]);
                    }
                }
            }
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
        }

        private int GetCellVertKey(int x, int y, int cell_x, int cell_y)
        {
            return (cell_y << (cellsPerRowPowerOfTwo + 4)) + (cell_x << 4) + (y << 2) + x;
        }

        private static Vector3 GetCellVertPos(float x, float y, int cell_x, int cell_y, float cellSize)
        {
            return new Vector3(
                    cellSize * (x / 2.0f + cell_x),
                    cellSize * (y / 2.0f + cell_y),
                    0
                );
        }
    }


    public struct CellSubmesh
    {
        public float[] vertices;
        public int[] triangles;
    }
}
