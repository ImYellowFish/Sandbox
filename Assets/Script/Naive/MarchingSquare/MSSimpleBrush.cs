using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingSquare
{
    [RequireComponent(typeof(MSTrunkRenderer))]
    public class MSSimpleBrush : MonoBehaviour
    {
        public float brushSize = 1.0f;
        public float brushIntensity = 1.0f;
        public Vector3 mousePosRaw;
        public Vector3 mousePos;

        public enum BrushType { Single, Area }
        public enum InputMode { Hold, Click }
        public BrushType brushType;
        public InputMode inputMode;

        private MSTrunkRenderer trunkRenderer;

        // Use this for initialization
        void Start()
        {
            trunkRenderer = GetComponent<MSTrunkRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            bool shouldPaint = false;
            float strength = 0f;
            switch (inputMode)
            {
                case InputMode.Hold:
                    strength = Time.deltaTime * brushIntensity;
                    shouldPaint = Input.GetMouseButton(0);
                    break;
                case InputMode.Click:
                    strength = brushIntensity;
                    shouldPaint = Input.GetMouseButtonDown(0);
                    break;
            }

            if (shouldPaint){
                mousePos = GetMouseWorldPos();
                switch (brushType)
                {
                    case BrushType.Area:
                        Paint(mousePos, strength);
                        break;
                    case BrushType.Single:
                        PaintSingle(mousePos, strength);
                        break;
                }
            }
        }

        private Vector3 GetMouseWorldPos(){
            var mousePosRaw = Input.mousePosition;
            this.mousePosRaw = mousePosRaw;
            mousePosRaw.z = 10;
            return Camera.main.ScreenToWorldPoint(mousePosRaw);
        }

        public void PaintSingle(Vector3 position, float strength)
        {
            var trunk = trunkRenderer.trunk;
            int coord_x, coord_y;
            trunk.GetGridIntCoordAtPos(position, out coord_x, out coord_y);
            //Debug.Log(coord_x.ToString() + ", " + coord_y.ToString());
            trunk.AddValueAtCoord(coord_x, coord_y, strength);
            trunkRenderer.UpdateMesh();
        }

        public void Paint(Vector3 position, float strength)
        {
            var trunk = trunkRenderer.trunk;
            var center = trunk.GetGridCoordAtPos(position);
            var coordRadius = brushSize / trunk.cellSize;
            var roundedRadius = Mathf.RoundToInt(coordRadius);
            for(int i = -roundedRadius; i <= roundedRadius; i++){
                for(int j = -roundedRadius; j <= roundedRadius; j++){
                    Vector2 delta = new Vector2(i, j);
                    float dist = delta.magnitude;
                    if(dist < roundedRadius){
                        var coord = delta + center;
                        var falloff = 0.8f * Mathf.Clamp01(1f - dist * dist / coordRadius / coordRadius);
                        trunk.AddValueAtCoord(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), falloff * strength);
                    }
                }
            }
            trunkRenderer.UpdateMesh();
        }
    }
}