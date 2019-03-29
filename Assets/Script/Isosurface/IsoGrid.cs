using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsoSurface
{
    public class IsoGrid : MonoBehaviour
    {
        public Int3 dimension = new Int3(20, 20, 20);
        public float cellSize = 0.1f;

        [System.NonSerialized]
        public float[,,] data;

        private void Awake()
        {
            data = new float[dimension.x, dimension.y, dimension.z];
        }

        public float this[Int3 index]
        {
            get
            {
                return data[index.x, index.y, index.z];
            }
            set
            {
                data[index.x, index.y, index.z] = value;
            }
        }

        [ContextMenu("Generate Sphere")]
        public void GenerateSphere()
        {
            float radius = dimension.x / 2.5f;
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        float dist = Vector3.Distance(new Vector3(i + 0.5f, j + 0.5f, k + 0.5f), dimension.ToVector3() / 2f);
                        data[i, j, k] = dist < radius ? 1f : 0f;
                    }
                }
            }
        }

        [ContextMenu("Generate Smooth Sphere")]
        public void GenerateSmooothSphere()
        {
            float radius = dimension.x / 4f;
            for (int i = 0; i < dimension.x; i++)
            {
                for (int j = 0; j < dimension.y; j++)
                {
                    for (int k = 0; k < dimension.z; k++)
                    {
                        float dist = Vector3.Distance(new Vector3(i+0.5f, j+0.5f, k+0.5f), dimension.ToVector3() / 2f);
                        data[i, j, k] = 1f - 0.5f * (dist / radius) * (dist / radius);
                    }
                }
            }
        }

    }
}