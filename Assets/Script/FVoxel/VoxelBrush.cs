using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel
{
    public abstract class VoxelBrushBase
    {
        public abstract void Apply(VoxelTrunk trunk, Vector3 centerPosition);
    }
    
    [System.Serializable]
    public class VoxelPaintBrush : VoxelBrushBase
    {
        public float radius;
        public float strength;
        public AnimationCurve curve;
        
        public override void Apply(VoxelTrunk trunk, Vector3 centerPosition)
        {
            var centerCoord = trunk.GetCoordByWorldPos(centerPosition);
            Int3 radiusByCell = new Int3(
                Mathf.RoundToInt(radius / trunk.cellSize.x),
                Mathf.RoundToInt(radius / trunk.cellSize.y),
                Mathf.RoundToInt(radius / trunk.cellSize.z));
            if (radiusByCell.x <= 0 || radiusByCell.y <= 0 || radiusByCell.z <= 0)
                // If brush radius is zero, skip.
                return;
            // Loop through all cells in brush range
            for(int i = -radiusByCell.x; i <= radiusByCell.x; i++)
            {
                for(int j = -radiusByCell.y; j <= radiusByCell.y; j++)
                {
                    for (int k = -radiusByCell.z; k <= radiusByCell.z; k++)
                    {
                        var coord = centerCoord.Offset(i, j, k);
                        //Debug.Log("Paint at coord:" + coord + ", Inside:" + trunk.data.ContainsCell(coord));
                        if (!trunk.data.ContainsCell(coord))
                            // If cell is not inside trunk, skip
                            continue;
                        float t = new Vector3((float)i / radiusByCell.x, (float)j / radiusByCell.y, (float)k / radiusByCell.z).magnitude;
                        if (t > 1)
                            continue;
                        
                        var currentStrength = curve.Evaluate(t) * strength;
                        trunk.data.PaintFill(coord, currentStrength);

                    }
                }
            }
        }
    }
}
