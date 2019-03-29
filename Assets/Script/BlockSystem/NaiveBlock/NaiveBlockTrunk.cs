using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock
{
    public class NaiveBlockTrunk : MonoBehaviour
    {
        public Cube[,,] cubes;
        public int trunkSize = 10;
        public float cubeSize = 1f;
        public int trunkHeight = 10;
        public Material sharedMaterial;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private void Awake()
        {
            cubes = new Cube[trunkSize, trunkHeight, trunkSize];
            SetCubesValue(1);
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = sharedMaterial;
            meshCollider = gameObject.AddComponent<MeshCollider>();
            UpdateMesh();
        }

        public void SetCubesValue(int fill)
        {
            for (int i = 0; i < trunkSize; i++)
            {
                for (int j = 0; j < trunkHeight; j++)
                {
                    for (int k = 0; k < trunkSize; k++)
                    {
                        cubes[i, j, k].fill = 1;
                    }
                }
            }
        }

        public void SetCubeAtPos(Vector3 pos, int value)
        {
            int x = (int)(pos.x / cubeSize);
            int y = (int)(pos.y / cubeSize);
            int z = (int)(pos.z / cubeSize);
            if(IsInsideCube(x,y,z))
                cubes[x, y, z].fill = value;
        }

        [ContextMenu("Update Mesh")]
        public void UpdateMesh()
        {
            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            for(int i = 0; i < trunkSize; i++)
            {
                for(int j = 0; j < trunkHeight; j++)
                {
                    for(int k = 0; k < trunkSize; k++)
                    {
                        if(IsTransparent(i,j,k) || IsOccludedByNeighbors(i, j, k))
                        {
                            continue;
                        }
                        //Debug.Log("Visible:" + new Vector3(i, j, k));
                        var vert_offset = GetCubeLeftBottomPosition(i, j, k);
                        var index_offset = vertices.Count;
                        foreach (var vert in NaiveBlockData.baseCubeVertices)
                        {
                            vertices.Add(vert + vert_offset);
                        }
                        foreach(var uv in NaiveBlockData.baseCubeUVs)
                        {
                            uvs.Add(uv);
                        }
                        foreach(var index in NaiveBlockData.baseCubeTriangles)
                        {
                            triangles.Add(index_offset + index);
                        }
                    }
                }
            }
            m.SetVertices(vertices);
            m.SetTriangles(triangles, 0);
            m.SetUVs(0, uvs);
            m.RecalculateNormals();
            m.RecalculateBounds();
            meshFilter.mesh = m;
            meshCollider.sharedMesh = m;
            Debug.Log("Mesh face count:" + triangles.Count);
        }

        public bool IsInsideCube(int i, int j, int k)
        {
            return i >= 0 && i < trunkSize &&
                j >= 0 && j < trunkHeight &&
                k >= 0 && k < trunkSize;
        }

        public bool IsOccludedByNeighbors(int i, int j, int k)
        {
            if (IsTransparent(i+1, j, k) || IsTransparent(i-1, j, k) ||
                IsTransparent(i, j+1, k) || IsTransparent(i, j-1, k) ||
                IsTransparent(i, j, k+1) || IsTransparent(i, j, k-1))
            {
                return false;
            }
            return true;
        }

        public bool IsTransparent(int i, int j, int k)
        {
            if (!IsInsideCube(i,j,k))
            {
                return true;
            }
            else
                return cubes[i, j, k].fill == 0;
        }

        public Vector3 GetCubeLeftBottomPosition(int i, int j, int k)
        {
            return new Vector3(i, j, k) * cubeSize;
        }
    }

    public struct Cube
    {
        public int fill;
    }
}