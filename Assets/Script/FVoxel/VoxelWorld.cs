using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {
    public class VoxelWorld : MonoBehaviour {
        private static VoxelWorld _instance;
        public static VoxelWorld Instance { get { return _instance; } }

         public VoxelTrunk[,,] terrainTrunks;
        public int trunksPerRow = 4;
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
            terrainTrunks = new VoxelTrunk[trunksPerRow, 1, trunksPerRow];
            var childCount = transform.childCount;
            for(int i = 0; i < childCount; i++)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            Vector3 trunkSize = Vector3.Scale(trunkCellSize, trunkDimension.ToVector3());
            for (int i = 0; i < trunksPerRow; i++)
            {
                for(int k = 0; k < trunksPerRow; k++)
                {
                    var trunkObj = new GameObject("trunk_" + i + "_" + k);
                    var coord = new Int3(i, 0, k);
                    trunkObj.transform.SetParent(transform);
                    trunkObj.transform.localPosition = Vector3.Scale(trunkSize, coord.ToVector3());
                    var trunk = trunkObj.AddComponent<VoxelTrunk>();
                    trunk.coordinate = coord;
                    trunk.dimension = trunkDimension;
                    trunk.cellSize = trunkCellSize;
                    trunk.material = material;
                    trunkObj.tag = "Voxel";
                    terrainTrunks[i, 0, k] = trunk;
                }
            }
        }

        [ContextMenu("Test")]
        public void Test()
        {
            foreach (var trunk in terrainTrunks)
            {
                var testor = trunk.gameObject.AddComponent<VoxelTrunkTestor>();
                testor.GenerateHalfCubeImpl();
            }
        }
    }
}