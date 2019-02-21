using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingSquare
{
    [RequireComponent(typeof(MSTrunkRenderer))]
    public class MSSimpleBrush : MonoBehaviour
    {
        public float brushSize = 1.0f;
        public float intensity = 1.0f;
        public Vector3 mousePosRaw;
        public Vector3 mousePos;

        private MSTrunkRenderer trunkRenderer;

        // Use this for initialization
        void Start()
        {
            trunkRenderer = GetComponent<MSTrunkRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetMouseButton(0)){
                mousePos = GetMouseWorldPos();
                Paint(mousePos);
            }
        }

        private Vector3 GetMouseWorldPos(){
            var mousePosRaw = Input.mousePosition;
            this.mousePosRaw = mousePosRaw;
            mousePosRaw.z = 10;
            return Camera.main.ScreenToWorldPoint(mousePosRaw);
        }

        public void Paint(Vector3 position){
            var trunk = trunkRenderer.trunk;
            var center = trunk.GetGridCoordAtPos(position);
            var coordRadius = brushSize / trunk.cellSize;
            var roundedRadius = Mathf.RoundToInt(coordRadius);
            for(int i = -roundedRadius; i <= roundedRadius; i++){
                for(int j = -roundedRadius; j <= roundedRadius; j++){
                    Vector2 delta = new Vector2(i, j);
                    float dist = delta.magnitude;
                    if(dist <= roundedRadius){
                        var coord = delta + center;
                        var falloff = Mathf.SmoothStep(0, roundedRadius, dist);
                        trunk.AddValueAtCoord(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), falloff * intensity * Time.deltaTime);
                    }
                }
            }
            trunkRenderer.UpdateMesh();
        }
    }
}