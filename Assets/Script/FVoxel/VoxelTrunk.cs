using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {
    public class VoxelTrunk : MonoBehaviour {
        public Int3 coordinate = Int3.Zero;
        public Int3 dimension = new Int3(10,10,10);
        public Vector3 cellSize = Vector3.one;

        public VoxelData data;
        public TriangulationSolver solver;

        public Material material;

        [HideInInspector]
        public MeshRenderer meshRenderer;
        [HideInInspector]
        public MeshFilter meshFilter;
        [HideInInspector]
        public MeshCollider meshCollider;

        [Header("Readonly")]
        public int triangleCount;
        public Mesh mesh;

        private void Awake()
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            mesh = new Mesh();
            mesh.name = "TrunkMesh";
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        private void Start()
        {
            InitVoxelData();
            meshRenderer.material = material;
            solver = new SurfaceNetTrigSolver(this);
        }

        public void InitVoxelData()
        {
            data = new VoxelData(dimension, cellSize);
        }

        [ContextMenu("Triangulate")]
        public void Triangulate()
        {
            solver.Solve(mesh);
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = mesh;
            triangleCount = mesh.triangles.Length / 3;
            data.isDirty = false;
        }

        public Int3 GetCoordByWorldPos(Vector3 worldPosition)
        {
            var localPos = (worldPosition - transform.position);
            return new Int3((int)(localPos.x / cellSize.x), 
                (int)(localPos.y / cellSize.y), (int)(localPos.z / cellSize.z));
        }
    }
}