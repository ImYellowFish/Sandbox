using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FILL_VALUE_TYPE = System.Byte;

namespace FVoxel
{    
    public class SurfaceNetTrigSolver : TriangulationSolver
    {
        #region Internal variables
        public Int3 dimension { get { return base.data.dimension; } }
        private List<Vector3> vertices;
        private List<int> triangles;
        private List<Vector3> normals;
        public Dictionary<int, int> vertIndexBuffer;

        // Lookups:
        // 8 cell vertices position arranged with vertex indices in 0~7
        private Vector3[] vertexPosLookup;
        // List of edge interection points. [edgeVert0A, edgeVert0B, edgeVert1A, edgeVert1B...]
        private List<int>[] intersectionVertLookup;
        // List of edge intersection count along each of the 3 axis
        private int[,] intersectionAxisLookup;
        // List of the coordinate offsets for 8 vertices in the same cell
        private Int3[] cellOffsetsLookup;
        #endregion
        
        public SurfaceNetTrigSolver(VoxelTrunk trunk): base(trunk)
        {
            // Stores all the lookups
            var lookups = SurfaceNetTrigLookups.Instance;
            vertexPosLookup = lookups.vertexPosLookup;
            intersectionVertLookup = lookups.intersectionVertLookup;
            intersectionAxisLookup = lookups.intersectionAxisLookup;
            cellOffsetsLookup = lookups.cellOffsetsLookup;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            normals = new List<Vector3>();
            vertIndexBuffer = new Dictionary<int, int>();
        }

        public void ResetSolver()
        {
            // Clear all the buffers
            vertices.Clear();
            triangles.Clear();
            normals.Clear();

            // Reset mesh vertex index lookup
            vertIndexBuffer.Clear();
        }

        public override void Solve(Mesh mesh)
        {
            ResetSolver();
            AddCrossBoundaryVertices();
            Int3 solveRegionMin = Int3.Zero;
            Int3 solveRegionMax = dimension.Offset(-1,-1,-1);

            for (int i = solveRegionMin.x; i <= solveRegionMax.x; i++)
            {
                for (int j = solveRegionMin.y; j <= solveRegionMax.y; j++)
                {
                    for (int k = solveRegionMin.z; k <= solveRegionMax.z; k++)
                    {
                        SolveCell(new Int3(i, j, k));
                    }
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
        }


        public override void SolveIncrement(Mesh mesh)
        {
            Int3 solveRegionMin = Int3.Max(data.dirtyRegionMin.Offset(-1, -1, -1), Int3.Zero);
            Int3 solveRegionMax = Int3.Max(data.dirtyRegionMax.Offset(1, 1, 1), dimension.Offset(-1, -1, -1));

            for (int i = solveRegionMin.x; i <= solveRegionMax.x; i++)
            {
                for (int j = solveRegionMin.y; j <= solveRegionMax.y; j++)
                {
                    for (int k = solveRegionMin.z; k <= solveRegionMax.z; k++)
                    {
                        SolveCell(new Int3(i, j, k));
                    }
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
        }

        private void SolveCell(Int3 cellCoord)
        {
            FILL_VALUE_TYPE[] cellFillValues = new FILL_VALUE_TYPE[8];
            int cellFillsMask = GetCellFillsAndMask(cellCoord, cellFillValues);
            if (cellFillsMask == 0 || cellFillsMask == 0xFF)
            {
                return;
            }

            // Intersections exist
            // Should add a mesh vertex in this cell
            var intersections = intersectionVertLookup[cellFillsMask];

            // store vertex index
            SetVertIndexAtCoord(cellCoord, vertices.Count);

            // Calculate mesh vertex position
            Vector3 averagedLocalPos = GetAverageIntersection(cellFillValues, intersections);
            Vector3 averagedWorldPos = Vector3.Scale(averagedLocalPos, data.cellSize) + GetCellOriginPos(cellCoord);
            vertices.Add(averagedWorldPos);

            normals.Add(ComputeCellNormal(cellCoord, cellFillValues, averagedLocalPos));
            // Add triangles in three dimensions
            for (int axis = 0; axis < 3; axis++)
            {
                // If no intersection along this axis, skip.
                if (intersectionAxisLookup[cellFillsMask, axis] == 0)
                    continue;
                int axis_1 = (axis + 1) % 3;
                int axis_2 = (axis + 2) % 3;

                var vi0 = GetVertIndexAtCoord(cellCoord);
                var vi1 = GetVertIndexAtCoord(cellCoord.Offset(axis_1, -1));
                var vi2 = GetVertIndexAtCoord(cellCoord.Offset(axis_1, -1).Offset(axis_2, -1));
                var vi3 = GetVertIndexAtCoord(cellCoord.Offset(axis_2, -1));
                if (vi0 < 0 || vi1 < 0 || vi2 < 0 || vi3 < 0)
                {
                    continue;
                }

                // Flip faces based on corner value.
                if ((cellFillsMask & 1) == 1)
                {
                    AddQuad(vi0, vi1, vi2, vi3);
                }
                else
                {
                    AddQuad(vi0, vi3, vi2, vi1);
                }
            }

        }

        private void AddCrossBoundaryVertices()
        {
            for(int axis = 0; axis < 3; axis++)
            {
                int axis_1 = (axis + 1) % 3;
                int axis_2 = (axis + 2) % 3;
                // Add diagonal
                for (int i = 0; i < dimension[axis]; i++)
                {
                    Int3 cellCoord = Int3.Zero;
                    cellCoord[axis] = i;
                    cellCoord[axis_1] = -1;
                    cellCoord[axis_2] = -1;

                    Vector3 vertexPosOffset = Vector3.zero;
                    vertexPosOffset[axis_1] = -trunk.trunkSize[axis_1];
                    vertexPosOffset[axis_2] = -trunk.trunkSize[axis_2];
                    AddCrossBoundaryVertexAtCell(cellCoord, vertexPosOffset);
                }

                // Add adjacent
                for (int j = 0; j < dimension[axis_1]; j++)
                {
                    for (int k = 0; k < dimension[axis_2]; k++)
                    {
                        Int3 cellCoord = Int3.Zero;
                        cellCoord[axis] = -1;
                        cellCoord[axis_1] = j;
                        cellCoord[axis_2] = k;

                        Vector3 vertexPosOffset = Vector3.zero;
                        vertexPosOffset[axis] = -trunk.trunkSize[axis];
                        AddCrossBoundaryVertexAtCell(cellCoord, vertexPosOffset);
                    }
                }
            }
        }

        private void AddCrossBoundaryVertexAtCell(Int3 cellCoord, Vector3 vertexPosOffset)
        {
            VoxelTrunk otherTrunk;
            // Get boundary cell info
            var otherCoord = trunk.GetCrossBoundaryCellInfo(cellCoord, out otherTrunk);
            if (otherTrunk == null)
                return;
            // Get boundary vertex info
            var otherSolver = otherTrunk.solver as SurfaceNetTrigSolver;
            var otherVertIndex = otherSolver.GetVertIndexAtCoord(otherCoord);
            if (otherVertIndex < 0)
                // No vertex at this cell, skip.
                return;
            var vertexPos = otherSolver.vertices[otherVertIndex];
            vertexPos += vertexPosOffset;
            var vertexNormal = otherSolver.normals[otherVertIndex];
            // Add boundary vertex to buffer
            SetVertIndexAtCoord(cellCoord, vertices.Count);
            vertices.Add(vertexPos);
            normals.Add(vertexNormal);
        }

        private int GetCellFillsMask(Int3 cellCoord)
        {
            int cellFillsMask = 0;
            for (int i = 0; i < cellOffsetsLookup.Length; i++)
            {
                var vertCoord = cellCoord + cellOffsetsLookup[i];
                var fill = data.GetFill(vertCoord);
                cellFillsMask += (fill >= data.fillThreshold ? (1 << i) : 0);
            }
            return cellFillsMask;
        }

        private Vector3 GetCellOriginPos(Int3 coord)
        {
            return Vector3.Scale(data.cellSize, coord.ToVector3());
        }

        private Vector3 ComputeCellNormal(Int3 cellCoord, FILL_VALUE_TYPE[] cellFillValues, Vector3 averagedLocalPos)
        {
            Vector3 normal;
            // Smoother normal
            normal.x = (GetCellFillsSum(cellCoord.Offset(-1, 0, 0), averagedLocalPos, 0) -
                GetCellFillsSum(cellCoord.Offset(1, 0, 0), averagedLocalPos, 0));
            normal.y = (GetCellFillsSum(cellCoord.Offset(0, -1, 0), averagedLocalPos, 1) -
                GetCellFillsSum(cellCoord.Offset(0, 1, 0), averagedLocalPos, 1));
            normal.z = (GetCellFillsSum(cellCoord.Offset(0, 0, -1), averagedLocalPos, 2) -
                GetCellFillsSum(cellCoord.Offset(0, 0, 1), averagedLocalPos, 2));

            return normal.normalized;
        }

        private int GetCellFillsSum(Int3 coord, Vector3 averagedLocalPos, int dim)
        {
            int sum = 0;
            //int dim_1 = (dim + 1) % 3;
            //int dim_2 = (dim + 2) % 3;
            for (int i = 0; i < cellOffsetsLookup.Length; i++)
            {
                var vertCoord = coord + cellOffsetsLookup[i];
                if (data.ContainsCell(vertCoord))
                {
                    sum += data.GetFill(vertCoord);
                }
                else
                {
                    sum += data.GetCrossBoundaryFill(vertCoord);
                }
            }
            return sum;
        }

        private void AddQuad(int vi0, int vi1, int vi2, int vi3)
        {
            triangles.Add(vi0);
            triangles.Add(vi1);
            triangles.Add(vi2);
            triangles.Add(vi2);
            triangles.Add(vi3);
            triangles.Add(vi0);
        }
        
        private Vector3 GetAverageIntersection(FILL_VALUE_TYPE[] cellFills, List<int> intersectionVertList)
        {
            Vector3 average = Vector3.zero;
            int count = 0;
            for (int i = 0; i < intersectionVertList.Count; i += 2)
            {
                int vert0 = intersectionVertList[i];
                int vert1 = intersectionVertList[i + 1];
                var fill0 = cellFills[vert0];
                var fill1 = cellFills[vert1];
                float t = fill1 - fill0;
                if (Mathf.Abs(t) > Mathf.Epsilon)
                {
                    t = (data.fillThreshold - fill0) / t;
                }
                else
                {
                    t = 0.5f;
                }
                average += Vector3.Lerp(vertexPosLookup[vert0], vertexPosLookup[vert1], t);
                count++;
            }
            return average / count;
        }

        private int GetCellFillsAndMask(Int3 coord, FILL_VALUE_TYPE[] cellFills)
        {
            int cellFillsMask = 0;
            for (int i = 0; i < cellOffsetsLookup.Length; i++)
            {
                var vertCoord = coord + cellOffsetsLookup[i];
                if (vertCoord < dimension)
                {
                    // Internel cells
                    cellFills[i] = data.GetFill(vertCoord);
                }
                else
                {
                    // Boundary cells
                    cellFills[i] = data.GetCrossBoundaryFill(vertCoord);
                }
                cellFillsMask += (cellFills[i] >= data.fillThreshold ? (1 << i) : 0);
            }
            return cellFillsMask;
        }

        private int GetVertIndexBufferKey(Int3 coord)
        {
            //Debug.Log("Coord: " + coord + ", Key:" + (coord.x + coord.y << 8 + coord.z << 16));
            int key = 0;
            if(coord.x >= 0)
            {
                key += coord.x;
            }
            else
            {
                key += coord.x + (1 << 9);
            }
            if(coord.y >= 0)
            {
                key += (Mathf.Abs(coord.y) << 10);
            }
            else
            {
                key += (Mathf.Abs(coord.y) << 10) + (1 << 19);
            }
            if(coord.z >= 0)
            {
                key += (Mathf.Abs(coord.z) << 20);
            }
            else
            {
                key += (Mathf.Abs(coord.z) << 20) + (1 << 29);
            }
            return key;
        }
        
        private void SetVertIndexAtCoord(Int3 coord, int value)
        {
            vertIndexBuffer[GetVertIndexBufferKey(coord)] = value;
        }

        private int GetVertIndexAtCoord(Int3 coord)
        {
            int key = GetVertIndexBufferKey(coord);
            if (vertIndexBuffer.ContainsKey(key))
            {
                return vertIndexBuffer[key];
            }
            return -1;
        }
    }
}