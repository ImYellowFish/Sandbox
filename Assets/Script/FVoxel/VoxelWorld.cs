using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {
    public class VoxelWorld : MonoBehaviour {
        private static VoxelWorld _instance;
        public static VoxelWorld Instance { get { return _instance; } }

        public VoxelTrunk[,,] terrainTrunks;
        public Int3 worldDimension = new Int3(4,4,4);
        public Material material;
        public Int3 trunkDimension = new Int3(10, 10, 10);
        public Vector3 trunkCellSize = Vector3.one;
        
        private void Awake()
        {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            InitTrunks();
        }

        public void InitTrunks()
        {
            terrainTrunks = new VoxelTrunk[worldDimension.x, worldDimension.y, worldDimension.z];
            var childCount = transform.childCount;
            for(int i = 0; i < childCount; i++)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            Vector3 trunkSize = Vector3.Scale(trunkCellSize, trunkDimension.ToVector3());
            for (int i = 0; i < worldDimension.x; i++)
            {
                for (int j = 0; j < worldDimension.y; j++)
                {
                    for (int k = 0; k < worldDimension.z; k++)
                    {
                        var trunkObj = new GameObject("trunk_" + i + "_" + j + "_" + k);
                        var coord = new Int3(i, j, k);
                        trunkObj.transform.SetParent(transform);
                        trunkObj.transform.localPosition = Vector3.Scale(trunkSize, coord.ToVector3());
                        var trunk = trunkObj.AddComponent<VoxelTrunk>();
                        trunk.world = this;
                        trunk.coordinate = coord;
                        trunk.dimension = trunkDimension;
                        trunk.cellSize = trunkCellSize;
                        trunk.material = material;
                        trunkObj.tag = "Voxel";
                        terrainTrunks[i, j, k] = trunk;
                    }
                }
            }
        }

        public bool ContainsTrunk(Int3 coord)
        {
            return coord >= Int3.Zero && coord < worldDimension;
        }

        public VoxelTrunk GetTrunk(Int3 coord)
        {
            try
            {
                if (ContainsTrunk(coord))
                    return terrainTrunks[coord.x, coord.y, coord.z];
                else
                    return null;
            }catch(System.IndexOutOfRangeException e)
            {
                Debug.LogError("Trunk not in world range:" + coord);
                throw e;
            }
        }
        
        [ContextMenu("Test")]
        public void Test()
        {
            foreach (var trunk in terrainTrunks)
            {
                var testor = trunk.gameObject.AddComponent<VoxelTrunkTestor>();
                if (trunk.coordinate.y == worldDimension.y - 1)
                    testor.GenerateHalfCubeImpl();
                else
                    testor.GenerateFullCubeImpl();
            }
            foreach (var trunk in terrainTrunks)
            {
                trunk.Triangulate();
            }
        }
    }
}