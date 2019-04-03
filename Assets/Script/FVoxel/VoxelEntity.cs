using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
// 1. FBX to voxel
// 2. UV mapping
// 3. LOD
namespace FVoxel {
    public class VoxelEntity : MonoBehaviour {
        public VoxelTrunk[,,] trunks;
        public Int3 worldDimension = new Int3(4, 4, 4);
        public Int3 trunkDimension = new Int3(10, 10, 10);
        public Vector3 trunkCellSize = Vector3.one;
        public Material material;
        
        private void Awake()
        {
            InitTrunks();
        }

        public void InitTrunks()
        {
            trunks = new VoxelTrunk[worldDimension.x, worldDimension.y, worldDimension.z];
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
                        trunk.ownerEntity = this;
                        trunk.coordinate = coord;
                        trunk.dimension = trunkDimension;
                        trunk.cellSize = trunkCellSize;
                        trunk.material = material;
                        trunkObj.tag = "Voxel";
                        trunks[i, j, k] = trunk;
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
                    return trunks[coord.x, coord.y, coord.z];
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
            foreach (var trunk in trunks)
            {
                var testor = trunk.gameObject.AddComponent<VoxelTrunkTestor>();
                if (trunk.coordinate.y == worldDimension.y - 1)
                    testor.GenerateHalfCubeImpl();
                else
                    testor.GenerateFullCubeImpl();
            }
            foreach (var trunk in trunks)
            {
                trunk.Triangulate();
            }
        }
    }
}