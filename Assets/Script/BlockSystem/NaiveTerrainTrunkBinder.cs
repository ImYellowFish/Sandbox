using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock
{
    public class NaiveTerrainTrunkBinder : MonoBehaviour
    {
        private NaiveBlockTrunk trunk;
        private NaiveBlockTerrain terrain;
        private BoxCollider boxCollider;

        public int terrainHeight = 2;
        public float distanceOffset = 0.01f;

        // Use this for initialization
        void Start()
        {
            trunk = FindObjectOfType<NaiveBlockTrunk>();
            terrain = FindObjectOfType<NaiveBlockTerrain>();
            for(int i = 0; i < trunk.trunkSize; i++)
            {
                for(int k = 0; k < trunk.trunkSize; k++)
                {
                    for(int j = 0; j < terrainHeight; j++)
                    {
                        trunk.cubes[i, j, k].fill = 1;
                    }
                    for (int j = terrainHeight; j < trunk.trunkHeight; j++)
                    {
                        trunk.cubes[i, j, k].fill = 0;
                    }
                }
            }

            terrain.transform.position = Vector2.up * trunk.cubeSize * terrainHeight;
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(trunk.trunkSize * trunk.cubeSize,
                trunk.cubeSize,
                trunk.trunkSize * trunk.cubeSize);
            boxCollider.center = Vector3.up * trunk.cubeSize * (terrainHeight + 0.5f);
            boxCollider.isTrigger = true;

            terrain.UpdateMesh();
            trunk.UpdateMesh();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var cam = Camera.main;
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                var isHit = Physics.Raycast(ray, out hitInfo, 1 << gameObject.layer);
                if (isHit)
                {
                    var pos = ray.GetPoint(hitInfo.distance + distanceOffset);
                    var oldValue = terrain.GetCubeAtPos(pos);
                    Debug.Log(oldValue);
                    if (oldValue > 0)
                    {
                        terrain.SetCubeAtPos(pos, 0);
                        terrain.UpdateMesh();
                        return;
                    }
                }

                isHit = Physics.Raycast(ray, out hitInfo, 99f, 1 << trunk.gameObject.layer);
                if (isHit)
                {
                    Debug.Log("ray cast hit");
                    var pos = ray.GetPoint(hitInfo.distance + distanceOffset);
                    trunk.SetCubeAtPos(pos, 0);
                    trunk.UpdateMesh();
                    return;
                }
            }
        }
    }
}