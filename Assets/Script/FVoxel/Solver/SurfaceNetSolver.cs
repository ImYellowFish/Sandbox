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
        private int[,,] vertIndexBuffer;

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
            vertIndexBuffer = new int[dimension.x, dimension.y, dimension.z];
        }

        public void ResetSolver()
        {
            // Clear all the buffers
            vertices.Clear();
            triangles.Clear();
            normals.Clear();

            // Reset mesh vertex index lookup
            vertIndexBuffer = new int[dimension.x, dimension.y, dimension.z];
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        vertIndexBuffer[i, j, k] = -1;
                    }
                }
            }
        }

        public override void Solve(Mesh mesh)
        {
            ResetSolver();

            for (int i = 0; i < dimension.x - 1; i++)
            {
                for (int j = 0; j < dimension.y - 1; j++)
                {
                    for (int k = 0; k < dimension.z - 1; k++)
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
            vertIndexBuffer[cellCoord.x, cellCoord.y, cellCoord.z] = vertices.Count;

            // Calculate mesh vertex position
            Vector3 averagedLocalPos = GetAverageIntersection(cellFillValues, intersections);
            Vector3 averagedWorldPos = Vector3.Scale(averagedLocalPos, data.cellSize) + GetCellOriginPos(cellCoord);
            vertices.Add(averagedWorldPos);

            normals.Add(ComputeCellNormal(cellCoord, averagedLocalPos));
            // Add triangles in three dimensions
            for (int axis = 0; axis < 3; axis++)
            {
                // If no intersection along this axis, skip.
                if (intersectionAxisLookup[cellFillsMask, axis] == 0)
                    continue;
                int axis_1 = (axis + 1) % 3;
                int axis_2 = (axis + 2) % 3;
                if (cellCoord[axis_1] == 0 || cellCoord[axis_2] == 0)
                {
                    // Skip if on boundary
                    continue;
                }

                var vi0 = GetCellVertexIndex(cellCoord);
                var vi1 = GetCellVertexIndex(cellCoord.Offset(axis_1, -1));
                var vi2 = GetCellVertexIndex(cellCoord.Offset(axis_1, -1).Offset(axis_2, -1));
                var vi3 = GetCellVertexIndex(cellCoord.Offset(axis_2, -1));
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

        private Vector3 ComputeCellNormal(Int3 cellCoord, Vector3 averagedLocalPos)
        {
            Vector3 normal;
            //float wx1 = averagedLocalPos.x;
            //float wx0 = 1f - wx1;
            //float wy1 = averagedLocalPos.y;
            //float wy0 = 1f - wy1;
            //float wz1 = averagedLocalPos.z;
            //float wz0 = 1f - wz1;
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
                //float weight = Mathf.Abs(1f - CELL_OFFSETS[i][dim_1] - averagedLocalPos[dim_1]) *
                //    Mathf.Abs(1f - CELL_OFFSETS[i][dim_2] - averagedLocalPos[dim_2]);
                sum += data.GetFill(vertCoord.Clamp(Int3.Zero, dimension.Offset(-1, -1, -1)));
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

        private int GetCellVertexIndex(Int3 coord)
        {
            //Debug.Log(coord);
            return vertIndexBuffer[coord.x, coord.y, coord.z];
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
                cellFills[i] = data.GetFill(vertCoord);
                cellFillsMask += (cellFills[i] >= data.fillThreshold ? (1 << i) : 0);
            }
            return cellFillsMask;
        }
    }
}