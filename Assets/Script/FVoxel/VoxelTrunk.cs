﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {
    public class VoxelTrunk : MonoBehaviour {
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

        /// <summary>
        /// The world that this trunk belongs to.
        /// </summary>
        [Header("Readonly")]
        public VoxelWorld world;
        /// <summary>
        /// The trunk's coordinate inside the world.
        /// TODO: change to VoxelEntity.
        /// </summary>
        public Int3 coordinate = Int3.Zero;
        public int triangleCount;
        public Mesh mesh;

        public Vector3 trunkSize { get { return Vector3.Scale(cellSize, dimension.ToVector3()); } }

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
            data = new VoxelData(this, dimension, cellSize);
        }

        [ContextMenu("Triangulate")]
        public void Triangulate()
        {
            solver.Solve(mesh);
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = mesh;
            triangleCount = mesh.triangles.Length / 3;
            data.ClearDirty();
        }

        public Int3 GetCoordByWorldPos(Vector3 worldPosition)
        {
            var localPos = (worldPosition - transform.position);
            return new Int3((int)(localPos.x / cellSize.x), 
                (int)(localPos.y / cellSize.y), (int)(localPos.z / cellSize.z));
        }

        /// <summary>
        /// Get the info of a cell which belongs to neighboring trunks.
        /// </summary>
        public Int3 GetCrossBoundaryCellInfo(Int3 cellCoord, out VoxelTrunk otherTrunk)
        {
            if(world == null)
            {
                // No world assigned, so there is no adjacent trunks info, return null value
                otherTrunk = null;
                return Int3.Zero;
            }
            var otherTrunkCoord = coordinate;
            var otherCellCoord = cellCoord;
            for(int dim = 0; dim < 3; dim++)
            {
                if (cellCoord[dim] >= dimension[dim])
                {
                    otherTrunkCoord = otherTrunkCoord.Offset(dim, 1);
                    otherCellCoord = otherCellCoord.Offset(dim, -dimension.x);
                } else if(cellCoord[dim] < 0)
                {
                    otherTrunkCoord = otherTrunkCoord.Offset(dim, -1);
                    otherCellCoord = otherCellCoord.Offset(dim, dimension[dim]);
                }
            }
            otherTrunk = world.GetTrunk(otherTrunkCoord);
            //if(otherCellCoord.y < 0)
            //    Debug.Log("before:" + cellCoord + ", after: " + otherCellCoord);
            return otherCellCoord;
        }

        public void GetNearbyTrunksAtPos(Vector3 worldPos, Vector3 distanceLimit, List<VoxelTrunk> trunks)
        {
            trunks.Clear();
            var cellCoord = GetCoordByWorldPos(worldPos);
            var limit = new Int3(
                    Mathf.CeilToInt(distanceLimit.x / cellSize.x),
                    Mathf.CeilToInt(distanceLimit.y / cellSize.y),
                    Mathf.CeilToInt(distanceLimit.z / cellSize.z)
                );
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        var offset = new Int3(i * limit.x, j * limit.y, k * limit.z);
                        var nearbyCellCoord = cellCoord + offset;
                        if (!data.ContainsCell(nearbyCellCoord))
                        {
                            var neighbor = world.GetTrunk(coordinate.Offset(i,j,k));
                            if (neighbor && !trunks.Contains(neighbor))
                                trunks.Add(neighbor);
                        }
                        else if(i == 0 && j == 0 && k == 0)
                        {
                            trunks.Add(this);
                        }
                    }
                }
            }
        }
    }
}