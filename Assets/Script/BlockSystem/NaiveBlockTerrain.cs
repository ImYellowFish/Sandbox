using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock {
    public class NaiveBlockTerrain : MonoBehaviour {
        public Terrain referenceTerrain;
        public NaiveBlockTrunk trunk;

        public int terrainCubePerRow = 10;
        public int cubeMeshGridSize = 10;
        public float heightScale = 1f;
        public float cubeSize = 1f;

        private TerrainData raw_data;
        public int raw_width;
        public int raw_height;
        public Vector3 raw_scale;
        
        private void Awake()
        {
            raw_data = referenceTerrain.terrainData;
            raw_height = raw_data.heightmapHeight;
            raw_width = raw_data.heightmapWidth;
            raw_scale = raw_data.heightmapScale;
        }

        [ContextMenu("Update Mesh")]
        public void UpdateMesh()
        {
            Mesh m = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            for(int i = 0; i < raw_width - 1; i++)
            {
                for (int j = 0; j < raw_height - 1; j++)
                {
                    Vector3[] quad = new Vector3[4];
                    quad[0] = GetVertex(i, j);
                    quad[1] = GetVertex(i, j + 1);
                    quad[2] = GetVertex(i + 1, j + 1);
                    quad[3] = GetVertex(i + 1, j);
                }
            }
        }

        private Vector3 GetVertex(int i, int j)
        {
            Vector3 vert;
            vert.x = i * cubeSize / terrainCubePerRow;
            vert.y = j * cubeSize / terrainCubePerRow;
            vert.z = raw_data.GetHeight(i, j) * heightScale;
            return vert;
        }
    }
}