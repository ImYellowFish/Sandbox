using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingSquare
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MSTrunkRenderer : MonoBehaviour
    {
        public int cellsPerRow;
        public float cellSize;

        public Trunk trunk;
        private MeshFilter meshFilter;

        public float GetValueAtWorldPos(Vector2 pos)
        {
            var localPos = pos - (Vector2)(transform.position);
            return trunk.GetValueAtLocalPos(localPos);
        }

        public void SetValueAtWorldPos(Vector2 pos, float value)
        {
            var localPos = pos - (Vector2)(transform.position);
            trunk.SetValueAtPos(localPos, value);
        }

        private void Awake()
        {
            cellsPerRow = Mathf.ClosestPowerOfTwo(cellsPerRow);
            trunk = new Trunk(cellsPerRow, cellSize);
            meshFilter = GetComponent<MeshFilter>();
            InitGridValue(1f);
            UpdateMesh();
        }
        
        /// <summary>
        /// Update mesh based on the grid value
        /// </summary>
        public void UpdateMesh()
        {
            var mesh = meshFilter.mesh;
            if(mesh == null){
                mesh = new Mesh();
            }
            trunk.RecalculateTrunkMesh(mesh);
            meshFilter.mesh = mesh;
        }

        private void InitGridValue(float value)
        {
            trunk.InitGridValue(value);
        }
    }
}