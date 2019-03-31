using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock {
    public class NaiveBlockRaycast : MonoBehaviour {
        private NaiveBlockTrunk trunk;
        public float distanceOffset = 0.01f;
        public LayerMask layer;

        // Use this for initialization
        void Start() {
            trunk = GetComponent<NaiveBlockTrunk>();
    }

        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButtonDown(0))
            {
                var cam = Camera.main;
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                var isHit = Physics.Raycast(ray, out hitInfo, layer.value);
                if (isHit)
                {
                    var pos = ray.GetPoint(hitInfo.distance + distanceOffset);
                    trunk.SetCubeAtPos(pos, 0);
                    trunk.UpdateMesh();
                }
            }
        }
    }
}