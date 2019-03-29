using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsoSurface
{
    public class SurfaceNetSolver : MonoBehaviour
    {
        public bool debug_showVertOnly;
        public bool debug_noInterpolate;
        public bool debug_showSufaceNetCubes;

        private IsoGrid grid;

        private Int3 dimension { get { return grid.dimension; } }

        private List<Vector3> vertices;
        private List<int> triangles;
        private List<Vector3> normals;
        private int[,,] meshVertexIndexLookup;
        
        // Lookups:
        // 8 cell vertices position arranged with vertex indices in 0~7
        public Vector3[] vertexPosLookup;
        // List of edge interection points. [edgeVert0A, edgeVert0B, edgeVert1A, edgeVert1B...]
        private List<int>[] intersectionVertLookup;
        // List of edge intersection count along each of the 3 axis
        private int[,] intersectionAxisLookup;

        private readonly float DEPTH_THRESHOLD = 0.5f;
        private readonly Int3[] CELL_OFFSETS = new Int3[]
        {
            new Int3(0,0,0),
            new Int3(1,0,0),
            new Int3(1,0,1),
            new Int3(0,0,1),
            new Int3(0,1,0),
            new Int3(1,1,0),
            new Int3(1,1,1),
            new Int3(0,1,1),
        };

        [ContextMenu("GenerateLookups")]
        public void GenerateLookups()
        {
            vertexPosLookup = new Vector3[8];
            for(int i = 0; i < 8; i++)
            {
                int xi = (i >> 1 & 1) ^ (i & 1);
                int yi = (i >> 2 & 1);
                int zi = (i >> 1 & 1);
                vertexPosLookup[i] = new Vector3(xi, yi, zi);
            }

            int[] cellDepths = new int[8];
            intersectionVertLookup = new List<int>[256];
            intersectionAxisLookup = new int[256, 3];
            for (int mask = 0; mask <= 0xFF; mask++)
            {
                for (int i = 0; i < 8; i++)
                    cellDepths[i] = mask >> i & 1;
                intersectionVertLookup[mask] = new List<int>();
                intersectionAxisLookup[mask, 0] = 0;
                intersectionAxisLookup[mask, 1] = 0;
                intersectionAxisLookup[mask, 2] = 0;
                for (int edge = 0; edge < 12; edge++)
                {
                    int vert0, vert1;
                    if (edge >= 8)
                    {
                        vert0 = edge - 8;
                        vert1 = edge - 4;
                    }
                    else if(edge >= 4)
                    {
                        vert0 = edge;
                        vert1 = (edge - 3) % 4 + 4;
                    }
                    else
                    {
                        vert0 = edge;
                        vert1 = (edge + 1) % 4;
                    }
                    int depth0 = cellDepths[vert0];
                    int depth1 = cellDepths[vert1];
                    if (depth0 != depth1)
                    {
                        intersectionVertLookup[mask].Add(vert0);
                        intersectionVertLookup[mask].Add(vert1);

                        var dirVec = vertexPosLookup[vert0] - vertexPosLookup[vert1];
                        int dirIndex = Mathf.RoundToInt(Mathf.Abs(dirVec[1])) * 1 + Mathf.RoundToInt(Mathf.Abs(dirVec[2])) * 2;
                        intersectionAxisLookup[mask, dirIndex]++;
                    }
                }
                //Debug.Log("mask:" + mask + "|| " + "intersection count:" + Utility.IterableToString(intersectionAxisLookup, mask));
                //Debug.Log("mask:" + mask + "|| " + Utility.IterableToString(intersectionVertLookup[mask]));
            }
        }

        public void Start()
        {
            grid = GetComponent<IsoGrid>();
            vertices = new List<Vector3>();
            triangles = new List<int>();
            normals = new List<Vector3>();
        }

        public void ResetSolver()
        {
            vertices.Clear();
            triangles.Clear();
            normals.Clear();

            meshVertexIndexLookup = new int[dimension.x, dimension.y, dimension.z];
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        meshVertexIndexLookup[i,j,k] = -1;
                    }
                }
            }
        }

        [ContextMenu("Solve")]
        public void Solve()
        {
            ResetSolver();

            for (int i = 0; i < dimension.x-1; i++)
            {
                for(int j = 0; j < dimension.y-1; j++)
                {
                    for(int k = 0; k < dimension.z-1; k++)
                    {
                        SolveCell(new Int3(i,j,k));
                    }
                }
            }
        }
        
        private void SolveCell(Int3 originCoord)
        {
            float[] cellDepths = new float[8];
            int cellDepthMask = GetCellDepthMask(originCoord, cellDepths);
            if (cellDepthMask == 0 || cellDepthMask == 0xFF)
            {
                return;
            }
            var intersections = intersectionVertLookup[cellDepthMask];
            
            // store vertex index
            meshVertexIndexLookup[originCoord.x, originCoord.y, originCoord.z] = vertices.Count;

            Vector3 averagedLocalPos = GetAverageIntersection(cellDepths, intersections);
            Vector3 averagedWorldPos = averagedLocalPos * grid.cellSize + GetCellOriginPos(originCoord);
            if(debug_noInterpolate)
                averagedWorldPos = GetCellOriginPos(originCoord) + Vector3.one * 0.5f * grid.cellSize;

            vertices.Add(averagedWorldPos);
            if (debug_showVertOnly)
            {
                vertices.Add(averagedWorldPos + 0.02f * Vector3.up);
                vertices.Add(averagedWorldPos + 0.02f * Vector3.right);
                vertices.Add(averagedWorldPos + 0.02f * Vector3.up + 0.02f * Vector3.right);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
                normals.Add(Vector3.forward);
            }
            else
            {
                normals.Add(ComputeCellNormal(originCoord, averagedLocalPos));
                // Add triangles in three dimensions
                for (int axis = 0; axis < 3; axis++)
                {
                    // If no intersection along this axis, skip.
                    if (intersectionAxisLookup[cellDepthMask, axis] == 0)
                        continue;
                    int axis_1 = (axis + 1) % 3;
                    int axis_2 = (axis + 2) % 3;
                    if (originCoord[axis_1] == 0 || originCoord[axis_2] == 0)
                    {
                        // Skip if on boundary
                        continue;
                    }
                    
                    var vi0 = GetCellVertexIndex(originCoord);
                    var vi1 = GetCellVertexIndex(originCoord.Offset(axis_1, -1));
                    var vi2 = GetCellVertexIndex(originCoord.Offset(axis_1, -1).Offset(axis_2, -1));
                    var vi3 = GetCellVertexIndex(originCoord.Offset(axis_2, -1));
                    if (vi0 < 0 || vi1 < 0 || vi2 < 0 || vi3 < 0)
                    {
                        continue;
                    }

                    // Flip faces based on corner value.
                    if ((cellDepthMask & 1) == 1)
                    {
                        AddQuad(vi0, vi1, vi2, vi3);
                    }
                    else
                    {
                        AddQuad(vi0, vi3, vi2, vi1);
                    }
                }
            }
        }

        private int GetCellDepthMask(Int3 cellCoord)
        {
            int cellDepthMask = 0;
            for (int i = 0; i < CELL_OFFSETS.Length; i++)
            {
                var vertCoord = cellCoord + CELL_OFFSETS[i];
                var depth = grid[vertCoord];
                cellDepthMask += (depth >= DEPTH_THRESHOLD ? (1 << i) : 0);
            }
            return cellDepthMask;
        }

        private Vector3 GetCellOriginPos(Int3 coord)
        {
            return grid.cellSize * coord.ToVector3();
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
            normal.x = (GetCellDepthSum(cellCoord.Offset(-1, 0, 0), averagedLocalPos, 0) -
                GetCellDepthSum(cellCoord.Offset(1, 0, 0), averagedLocalPos, 0));

            normal.y = (GetCellDepthSum(cellCoord.Offset(0, -1, 0), averagedLocalPos, 1) -
                GetCellDepthSum(cellCoord.Offset(0, 1, 0), averagedLocalPos, 1));

            normal.z = (GetCellDepthSum(cellCoord.Offset(0, 0, -1), averagedLocalPos, 2) -
                GetCellDepthSum(cellCoord.Offset(0, 0, 1), averagedLocalPos, 2));
            
            return normal.normalized;
        }

        private float GetCellDepthSum(Int3 coord, Vector3 averagedLocalPos, int dim)
        {
            float sum = 0;
            int dim_1 = (dim + 1) % 3;
            int dim_2 = (dim + 2) % 3;
            for (int i = 0; i < CELL_OFFSETS.Length; i++)
            {
                var vertCoord = coord + CELL_OFFSETS[i];
                //float weight = Mathf.Abs(1f - CELL_OFFSETS[i][dim_1] - averagedLocalPos[dim_1]) *
                //    Mathf.Abs(1f - CELL_OFFSETS[i][dim_2] - averagedLocalPos[dim_2]);
                sum += grid[vertCoord.Clamp(Int3.Zero, dimension.Offset(-1,-1,-1))];
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
            return meshVertexIndexLookup[coord.x, coord.y, coord.z];
        }

        private Vector3 GetAverageIntersection(float[] cellDepths, List<int> intersectionVertList)
        {
            Vector3 average = Vector3.zero;
            int count = 0;
            for(int i = 0; i < intersectionVertList.Count; i += 2)
            {
                int vert0 = intersectionVertList[i];
                int vert1 = intersectionVertList[i + 1];
                float depth0 = cellDepths[vert0];
                float depth1 = cellDepths[vert1];
                float t = depth1 - depth0;
                if(Mathf.Abs(t) > Mathf.Epsilon)
                {
                    t = (DEPTH_THRESHOLD - depth0) / t;
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

        public Material show_material;
        [ContextMenu("Show")]
        public void Show()
        {
            var mr = gameObject.AddComponent<MeshRenderer>();
            var mf = gameObject.AddComponent<MeshFilter>();
            mr.sharedMaterial = show_material;
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            Debug.Log("Triangles:" + triangles.Count / 3);
            mf.sharedMesh = mesh;
        }

        [ContextMenu("Recalculate Normal")]
        public void RecalculateNormal()
        {
            var mf = GetComponent<MeshFilter>();
            mf.sharedMesh.RecalculateNormals();
        }

        private int GetCellDepthMask(Int3 coord, float[] cellDepths)
        {
            int cellDepthMask = 0;
            for (int i = 0; i < CELL_OFFSETS.Length; i++)
            {
                var vertCoord = coord + CELL_OFFSETS[i];
                cellDepths[i] = grid[vertCoord];
                cellDepthMask += (cellDepths[i] >= DEPTH_THRESHOLD ? (1 << i) : 0);
            }
            return cellDepthMask;
        }

        private void OnDrawGizmosSelected()
        {
            if (debug_showSufaceNetCubes && grid != null && grid.data != null)
            {
                var color0 = Gizmos.color;
                Gizmos.color = new Color(0, 0, 1, 0.2f);
                for (int i = 0; i < dimension.x - 1; i++)
                {
                    for (int j = 0; j < dimension.y - 1; j++)
                    {
                        for (int k = 0; k < dimension.z - 1; k++)
                        {
                            var coord = new Int3(i, j, k);
                            int mask = GetCellDepthMask(coord);
                            if(mask != 0 && mask != 0xFF)
                            {
                                var size = grid.cellSize * Vector3.one;
                                var center = GetCellOriginPos(coord) + size * 0.5f;
                                Gizmos.DrawCube(center, size);
                                Gizmos.DrawWireCube(center, size);
                            }
                        }
                    }
                }
                Gizmos.color = color0;
            }
        }
    }
}