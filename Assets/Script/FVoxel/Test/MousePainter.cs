using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {    
    public class MousePainter : MonoBehaviour {
        public string brushKey = "Remove";

        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Paint(ray);
            }
        }

        private List<VoxelTrunk> affectedTrunkList = new List<VoxelTrunk>();

        private void Paint(Ray ray)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.CompareTag("Voxel"))
            {
                var collidedTrunk = hitInfo.transform.GetComponent<VoxelTrunk>();
                var pos = hitInfo.point;
                var brush = VoxelBrushLibrary.GetBrush(brushKey);
                collidedTrunk.GetNearbyTrunksAtPos(pos, Vector3.one * brush.radius, affectedTrunkList);
                foreach(var trunk in affectedTrunkList)
                    brush.Apply(trunk, pos);
                foreach (var trunk in affectedTrunkList)
                    trunk.Triangulate();
            }
        }
    }
}