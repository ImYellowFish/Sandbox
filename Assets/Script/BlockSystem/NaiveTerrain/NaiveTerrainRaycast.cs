using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveTerrain
{
    public class NaiveTerrainRaycast : MonoBehaviour
    {
        private NaiveTerrain terrain;
        private void Start()
        {
            terrain = GetComponent<NaiveTerrain>();
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray, out hitInfo))
                {
                    var pos = ray.GetPoint(hitInfo.distance + 0.01f);
                    terrain.DigAtPos(pos);
                }
            }
        }
    }
}