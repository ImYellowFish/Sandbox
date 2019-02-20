using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingSquare
{
    public class MarchingSquareTest : MonoBehaviour
    {
        [Header("Base Data")]
        [Space]
        [InspectorButton("TestBaseDataImpl", 200, true)]
        public bool TestBaseData;

        [Header("Base Recon")]
        public int baseMeshReconCaseIndex;

        [InspectorButton("TestBaseMeshReconImpl", 200, true)]
        public bool TestBaseMeshRecon;

        [InspectorButton("TestNextBaseMeshReconIndexImpl", 200, true)]
        public bool TestNextBaseReconIndex;

        [Header("Composite Recon")]
        [Range(1, 255)]
        public int compositeReconValue = 255;

        [InspectorButton("TestCompositeMeshReconImpl", 200, true)]
        public bool TestCompositeMeshRecon;



        [ContextMenu("Test Base Reconstruction")]
        public void TestBaseMeshReconImpl()
        {
            var meshFilter = InitMeshFilter();
            var mesh = meshFilter.mesh;
            Trunk trunk = new Trunk(1, 1);
            trunk.grid[0, 0] = (baseMeshReconCaseIndex & 1) * 255;
            trunk.grid[0, 1] = (baseMeshReconCaseIndex >> 1 & 1) * 255;
            trunk.grid[1, 1] = (baseMeshReconCaseIndex >> 2 & 1) * 255;
            trunk.grid[1, 0] = (baseMeshReconCaseIndex >> 3 & 1) * 255;
            trunk.RecalculateTrunkMesh(mesh);
            meshFilter.mesh = mesh;
        }

        public void TestNextBaseMeshReconIndexImpl()
        {
            baseMeshReconCaseIndex = (baseMeshReconCaseIndex + 1) % 16;
            TestBaseMeshReconImpl();
        }

        public void TestCompositeMeshReconImpl()
        {
            var meshFilter = InitMeshFilter();
            var mesh = meshFilter.mesh;
            Trunk trunk = new Trunk(4, 1);
            int v = compositeReconValue;
            trunk.grid = new int[,]
            {
                { v, v, v, v, v },
                { v, 0, 0, 0, v },
                { v, 0, 0, v, v },
                { v, 0, 0, 0, v },
                { v, v, v, v, v },
            };
            trunk.RecalculateTrunkMesh(mesh);
            meshFilter.mesh = mesh;
        }
        

        [ContextMenu("Test Base Data")]
        public void TestBaseDataImpl()
        {
            var meshFilter = InitMeshFilter();
            List<Vector3> verts = new List<Vector3>();
            List<int> triangles = new List<int>();
            for(int i = 0; i < 16; i++)
            {
                var verts_raw = MarchingSquareData.vertices[i];
                var triangles_raw = MarchingSquareData.triangles[i];
                int indexOffset = verts.Count;
                for (int vi = 0; vi < verts_raw.Length; vi += 2)
                {
                    var vert = new Vector3(verts_raw[vi] + i * 3, verts_raw[vi + 1], 0);
                    verts.Add(vert);
                }
                for (int ti = 0; ti < triangles_raw.Length; ti += 3)
                {
                    triangles.Add(triangles_raw[ti] + indexOffset);
                    triangles.Add(triangles_raw[ti + 1] + indexOffset);
                    triangles.Add(triangles_raw[ti + 2] + indexOffset);
                }
            }
            var mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.mesh = mesh;
        }

        private MeshFilter InitMeshFilter()
        {
            var meshRenderer = GetOrAddComponent<MeshRenderer>();
            var meshFilter = GetOrAddComponent<MeshFilter>();
            var shader = Shader.Find("Mobile/Diffuse");
            meshRenderer.sharedMaterial = new Material(shader);
            return meshFilter;
        }

        private T GetOrAddComponent<T>() where T:Component
        {
            var cp = gameObject.GetComponent<T>();
            if (cp != null)
                return cp;
            else
                return gameObject.AddComponent<T>();
        }
    }
}