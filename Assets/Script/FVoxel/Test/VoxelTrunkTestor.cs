using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FILL_VALUE_TYPE = System.Byte;

namespace FVoxel
{
    public class VoxelTrunkTestor : MonoBehaviour
    {
        VoxelTrunk trunk;
        public VoxelData data
        {
            get { return trunk.data; }
        }
        public Int3 dimension
        {
            get { return trunk.data.dimension; }
        }


        // Use this for initialization
        void Awake()
        {
            trunk = GetComponent<VoxelTrunk>();
        }

        [InspectorButton("GenerateSphereImpl", ButtonWidth = 200)]
        public bool GenerateSphere;

        [InspectorButton("GenerateSmoothSphereImpl", ButtonWidth = 200)]
        public bool GenerateSmoothSphere;

        [InspectorButton("GenerateHalfCubeImpl", ButtonWidth = 200)]
        public bool GenerateHalfCube;

        [ContextMenu("Generate Sphere")]
        public void GenerateSphereImpl()
        {
            float radius = dimension.x / 2.5f;
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        float dist = Vector3.Distance(new Vector3(i + 0.5f, j + 0.5f, k + 0.5f), dimension.ToVector3() / 2f);
                        data.fill[i, j, k] = dist < radius ? FILL_VALUE_TYPE.MaxValue : FILL_VALUE_TYPE.MinValue;
                    }
                }
            }
            data.SetAllDirty();
            trunk.Triangulate();
        }

        [ContextMenu("Generate Smooth Sphere")]
        public void GenerateSmoothSphereImpl()
        {
            float radius = dimension.x / 4f;
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        float dist = Vector3.Distance(new Vector3(i + 0.5f, j + 0.5f, k + 0.5f), dimension.ToVector3() / 2f);
                        data.fill[i, j, k] = (FILL_VALUE_TYPE)(Mathf.Clamp01(1f - 0.5f * (dist / radius) * (dist / radius)) * 255);
                    }
                }
            }
            data.SetAllDirty();
            trunk.Triangulate();
        }

        [ContextMenu("Generate Half Cube")]
        public void GenerateHalfCubeImpl()
        {
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        data.fill[i, j, k] = (FILL_VALUE_TYPE)(int)((1f - (float)j / dimension.y) * FILL_VALUE_TYPE.MaxValue);
                        //data.fill[i, j, k] = j > dimension.y / 2 ? FILL_VALUE_TYPE.MinValue : FILL_VALUE_TYPE.MaxValue;
                        //Debug.Log(data.fill[i, j, k]);
                    }
                }
            }
            data.SetAllDirty();
            //trunk.Triangulate();
        }

        [ContextMenu("Generate Full Cube")]
        public void GenerateFullCubeImpl()
        {
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        data.fill[i, j, k] = FILL_VALUE_TYPE.MaxValue;
                    }
                }
            }
            data.SetAllDirty();
            //trunk.Triangulate();
        }
    }
}