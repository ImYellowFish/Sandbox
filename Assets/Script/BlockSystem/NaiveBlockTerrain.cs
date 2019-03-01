using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock {
    public class NaiveBlockTerrain : MonoBehaviour {
        public Terrain referenceTerrain;
        public NaiveBlockTrunk trunk;

        public int cubeMeshGridSize = 10;
        public float heightScale = 1f;
        public float cubeSize = 1f;

        private TerrainData raw_data;
        public int raw_width;
        public int raw_height;
        public Vector3 raw_scale;
        public int cubeCountPerRow;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        public Material sharedMaterial;

        private List<Vector3>[,] verticesArray;
        private List<int>[,] trianglesArray;
        private List<Vector3>[,] normalsArray;
        private List<Vector2>[,] uvsArray;

        [System.NonSerialized]
        public Cube[,] terrainCubeGrid;

        private GameObject lateralFaces;
        private MeshFilter lf_Filter;
        private MeshRenderer lf_Renderer;
        private MeshCollider lf_Collider;
        public Material lf_Material;

        private void Awake()
        {
            raw_data = referenceTerrain.terrainData;
            raw_height = raw_data.heightmapHeight;
            raw_width = raw_data.heightmapWidth;
            raw_scale = raw_data.heightmapScale;
            cubeCountPerRow = Mathf.FloorToInt(raw_scale.x * raw_width / cubeSize);

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshRenderer.sharedMaterial = sharedMaterial;

            verticesArray = new List<Vector3>[cubeCountPerRow, cubeCountPerRow];
            trianglesArray = new List<int>[cubeCountPerRow, cubeCountPerRow];
            normalsArray = new List<Vector3>[cubeCountPerRow, cubeCountPerRow];
            uvsArray = new List<Vector2>[cubeCountPerRow, cubeCountPerRow];

            terrainCubeGrid = new Cube[cubeCountPerRow, cubeCountPerRow];
            for(int i = 0; i < cubeCountPerRow; i++)
            {
                for(int j = 0; j < cubeCountPerRow; j++)
                {
                    terrainCubeGrid[i, j].fill = 1;
                }
            }

            lateralFaces = new GameObject("LateralFaces");
            lateralFaces.transform.SetParent(transform);
            lf_Filter = lateralFaces.AddComponent<MeshFilter>();
            lf_Renderer = lateralFaces.AddComponent<MeshRenderer>();
            lf_Collider = lateralFaces.AddComponent<MeshCollider>();
            lf_Renderer.sharedMaterial = lf_Material;

            InitCubeMesh();
            UpdateMesh();
        }

        [ContextMenu("Init Cube Mesh")]
        public void InitCubeMesh()
        {
            for(int i = 0; i < cubeCountPerRow; i++)
            {
                for (int j = 0; j < cubeCountPerRow; j++)
                {
                    verticesArray[i, j] = new List<Vector3>();
                    trianglesArray[i, j] = new List<int>();
                    normalsArray[i, j] = new List<Vector3>();
                    uvsArray[i, j] = new List<Vector2>();
                    InitCubeMesh(i, j, verticesArray[i, j], trianglesArray[i, j], normalsArray[i, j], uvsArray[i, j]);
                }
            }  
        }
        
        [ContextMenu("Update Mesh")]
        public void UpdateMesh()
        {
            // TODO: Auto vertex split if reaches limit
            Mesh m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            Mesh lf_m = new Mesh();
            var lf_vertices = new List<Vector3>();
            var lf_triangles = new List<int>();
            var lf_normals = new List<Vector3>();
            var lf_uvs = new List<Vector2>();
            var lf_uv2s = new List<Vector2>();
            var lf_colors = new List<Color>();

            for (int i = 0; i < cubeCountPerRow; i++)
            {
                for (int j = 0; j < cubeCountPerRow; j++)
                {
                    if(IsTransparent(i,j))
                    {
                        AddLateralFaces(i, j, lf_vertices, lf_triangles, lf_normals, lf_uvs, lf_uv2s, lf_colors);
                    }

                    else
                    {
                        AddCubeMeshToBatch(i, j, vertices, triangles, normals, uvs);
                    }
                }
            }
            m.SetVertices(vertices);
            m.SetTriangles(triangles, 0);
            m.SetNormals(normals);
            m.SetUVs(0, uvs);
            m.RecalculateBounds();
            meshFilter.mesh = m;
            meshCollider.sharedMesh = m;

            lf_m.SetVertices(lf_vertices);
            lf_m.SetTriangles(lf_triangles, 0);
            lf_m.SetNormals(lf_normals);
            lf_m.SetUVs(0, lf_uvs);
            lf_m.SetUVs(1, lf_uv2s);
            lf_m.SetColors(lf_colors);
            lf_m.RecalculateBounds();
            lf_Filter.mesh = lf_m;
            lf_Collider.sharedMesh = lf_m;
        }

        private void AddLateralFaces(int i, int j, List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs, List<Vector2> uv2s, List<Color> colors)
        {
            Vector3[] quad = new Vector3[4];
            float scanWidth = 1f / cubeMeshGridSize;
            if (!IsTransparent(i - 1, j))
            {
                for (int k = 0; k < cubeMeshGridSize; k++)
                {
                    quad[1] = GetVertex(i, j, 0, k);
                    quad[0] = new Vector3(quad[1].x, 0, quad[1].z);
                    quad[2] = GetVertex(i, j, 0, k + 1);
                    quad[3] = new Vector3(quad[2].x, 0, quad[2].z);

                    int index_offset = vertices.Count;
                    vertices.AddRange(quad);

                    var normal = new Vector3(1, 0, 0);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(GetVertexUV(i, j, 0, k) + Vector2.left * quad[1].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i, j, 0, k));
                    uvs.Add(GetVertexUV(i, j, 0, k + 1));
                    uvs.Add(GetVertexUV(i, j, 0, k + 1) + Vector2.left * quad[2].y / cubeCountPerRow);

                    uv2s.Add(new Vector2(scanWidth * k, 1));
                    uv2s.Add(new Vector2(scanWidth * k, 1 - quad[1].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1 - quad[2].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1));

                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 0f);

                    foreach (var index in NaiveBlockData.quadTriangles)
                    {
                        triangles.Add(index_offset + index);
                    }
                }
            }

            if (!IsTransparent(i + 1, j))
            {
                for (int k = 0; k < cubeMeshGridSize; k++)
                {
                    quad[0] = GetVertex(i + 1, j, 0, k);
                    quad[1] = new Vector3(quad[0].x, 0, quad[0].z);
                    quad[3] = GetVertex(i + 1, j, 0, k + 1);
                    quad[2] = new Vector3(quad[3].x, 0, quad[3].z);

                    int index_offset = vertices.Count;
                    vertices.AddRange(quad);

                    var normal = new Vector3(-1, 0, 0);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(GetVertexUV(i + 1, j, 0, k));
                    uvs.Add(GetVertexUV(i + 1, j, 0, k) + Vector2.right * quad[0].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i + 1, j, 0, k+1) + Vector2.right * quad[3].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i + 1, j, 0, k+1));

                    uv2s.Add(new Vector2(scanWidth * k, 1 - quad[0].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * k, 1));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1 - quad[3].y / cubeSize));

                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 1f);

                    foreach (var index in NaiveBlockData.quadTriangles)
                    {
                        triangles.Add(index_offset + index);
                    }
                }
            }

            if (!IsTransparent(i, j - 1))
            {
                for (int k = 0; k < cubeMeshGridSize; k++)
                {
                    quad[0] = GetVertex(i, j, k, 0);
                    quad[1] = new Vector3(quad[0].x, 0, quad[0].z);
                    quad[3] = GetVertex(i, j, k+1, 0);
                    quad[2] = new Vector3(quad[3].x, 0, quad[3].z);

                    int index_offset = vertices.Count;
                    vertices.AddRange(quad);

                    var normal = new Vector3(0, 0, 1);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(GetVertexUV(i, j, k, 0));
                    uvs.Add(GetVertexUV(i, j, k, 0) + Vector2.down * quad[0].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i, j, k+1, 0) + Vector2.down * quad[3].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i, j, k+1, 0));

                    uv2s.Add(new Vector2(scanWidth * k, 1 - quad[0].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * k, 1));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1 - quad[3].y / cubeSize));

                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 1f);

                    foreach (var index in NaiveBlockData.quadTriangles)
                    {
                        triangles.Add(index_offset + index);
                    }
                }
            }

            if (!IsTransparent(i, j + 1))
            {
                for (int k = 0; k < cubeMeshGridSize; k++)
                {
                    quad[1] = GetVertex(i, j + 1, k, 0);
                    quad[0] = new Vector3(quad[1].x, 0, quad[1].z);
                    quad[2] = GetVertex(i, j + 1, k + 1, 0);
                    quad[3] = new Vector3(quad[2].x, 0, quad[2].z);

                    int index_offset = vertices.Count;
                    vertices.AddRange(quad);

                    var normal = new Vector3(0, 0, -1);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(GetVertexUV(i, j + 1, k, 0) + Vector2.up * quad[1].y / cubeCountPerRow);
                    uvs.Add(GetVertexUV(i, j + 1, k, 0));
                    uvs.Add(GetVertexUV(i, j + 1, k+1, 0));
                    uvs.Add(GetVertexUV(i, j + 1, k+1, 0) + Vector2.up * quad[2].y / cubeCountPerRow);

                    uv2s.Add(new Vector2(scanWidth * k, 1));
                    uv2s.Add(new Vector2(scanWidth * k, 1 - quad[1].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1 - quad[2].y / cubeSize));
                    uv2s.Add(new Vector2(scanWidth * (k + 1), 1));

                    colors.Add(Color.red * 0f);
                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 1f);
                    colors.Add(Color.red * 0f);

                    foreach (var index in NaiveBlockData.quadTriangles)
                    {
                        triangles.Add(index_offset + index);
                    }
                }
            }
        }

        private void AddCubeMeshToBatch(int i, int j, List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs)
        {
            int index_offset = vertices.Count;
            vertices.AddRange(verticesArray[i, j]);
            normals.AddRange(normalsArray[i, j]);
            uvs.AddRange(uvsArray[i, j]);
            foreach (var index in trianglesArray[i, j])
            {
                triangles.Add(index + index_offset);
            }
        }

        private void InitCubeMesh(int i, int j, List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs)
        {
            for(int vi = 0; vi < cubeMeshGridSize; vi++)
            {
                for(int vj = 0; vj < cubeMeshGridSize; vj++)
                {
                    Vector3[] quad = new Vector3[4];
                    quad[0] = GetVertex(i, j, vi, vj);
                    quad[1] = GetVertex(i, j, vi, vj + 1);
                    quad[2] = GetVertex(i, j, vi + 1, vj + 1);
                    quad[3] = GetVertex(i, j, vi + 1, vj);

                    Vector3[] quadNormal = new Vector3[4];
                    quadNormal[0] = GetVertexNormal(i, j, vi, vj);
                    quadNormal[1] = GetVertexNormal(i, j, vi, vj + 1);
                    quadNormal[2] = GetVertexNormal(i, j, vi + 1, vj + 1);
                    quadNormal[3] = GetVertexNormal(i, j, vi + 1, vj);

                    uvs.Add(GetVertexUV(i, j, vi, vj));
                    uvs.Add(GetVertexUV(i, j, vi, vj + 1));
                    uvs.Add(GetVertexUV(i, j, vi + 1, vj + 1));
                    uvs.Add(GetVertexUV(i, j, vi + 1, vj));
                    
                    int index_offset = vertices.Count;
                    vertices.AddRange(quad);

                    foreach(var index in NaiveBlockData.quadTriangles)
                    {
                        triangles.Add(index_offset + index);
                    }

                    normals.AddRange(quadNormal);
                }
            }
        }

        public int GetCubeAtPos(Vector3 pos)
        {
            int x = (int)(pos.x / cubeSize);
            int z = (int)(pos.z / cubeSize);
            if (x >= 0 && x < cubeCountPerRow && z >= 0 && z < cubeCountPerRow)
                return terrainCubeGrid[x, z].fill;
            return 0;
        }

        public void SetCubeAtPos(Vector3 pos, int value)
        {
            int x = (int)(pos.x / cubeSize);
            int z = (int)(pos.z / cubeSize);
            if(x >= 0 && x < cubeCountPerRow && z >= 0 && z < cubeCountPerRow)
                terrainCubeGrid[x, z].fill = value;
        }

        private Vector3 GetVertex(int i, int j, int vi, int vj)
        {
            Vector3 vert;
            float gi = (i + (float)vi / cubeMeshGridSize);
            float gj = (j + (float)vj / cubeMeshGridSize);
            vert.y = raw_data.GetInterpolatedHeight(gi / cubeCountPerRow, gj / cubeCountPerRow);
            vert.x = gi * cubeSize;
            vert.z = gj * cubeSize;
            return vert;
        }

        private Vector2 GetVertexUV(int i, int j, int vi, int vj)
        {
            float gi = (i + (float)vi / cubeMeshGridSize);
            float gj = (j + (float)vj / cubeMeshGridSize);
            return new Vector2(gi / cubeCountPerRow, gj / cubeCountPerRow);
        }

        private Vector3 GetVertexNormal(int i, int j, int vi, int vj)
        {
            float gi = (i + (float)vi / cubeMeshGridSize);
            float gj = (j + (float)vj / cubeMeshGridSize);
            return raw_data.GetInterpolatedNormal(gi / cubeCountPerRow, gj / cubeCountPerRow);
        }

        private bool IsTransparent(int i, int j)
        {
            if (i < 0 || i >= cubeCountPerRow || j < 0 || j >= cubeCountPerRow)
                return true;
            return terrainCubeGrid[i,j].fill == 0;
        }
    }
}