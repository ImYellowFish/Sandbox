using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FILL_VALUE_TYPE = System.Byte;

namespace FVoxel {
    [System.Serializable]
    public class VoxelData {
        /// <summary>
        /// Whether a voxel is filled
        /// Range from 0 ~ 255
        /// </summary>
        [System.NonSerialized]
        public FILL_VALUE_TYPE[,,] fill;

        /// <summary>
        /// cells with fill values greater than this will be regarded as filled.
        /// </summary>
        public FILL_VALUE_TYPE fillThreshold = 127;

        public FILL_VALUE_TYPE fillMax { get { return FILL_VALUE_TYPE.MaxValue; } }
        public FILL_VALUE_TYPE fillMin { get { return FILL_VALUE_TYPE.MinValue; } }

        /// <summary>
        /// The cell count along x, y, z axis
        /// </summary>
        public Int3 dimension;

        /// <summary>
        /// The size of each cell in world units.
        /// </summary>
        public Vector3 cellSize = Vector3.one;

        /// <summary>
        /// Whether the grid data has changed since last triangulation
        /// </summary>
        public bool isDirty = false;

        public VoxelData(Int3 dimension, Vector3 cellSize)
        {
            this.dimension = dimension;
            this.cellSize = cellSize;
            fill = new byte[dimension.x, dimension.y, dimension.z];
        }

        public FILL_VALUE_TYPE GetFill(Int3 coord)
        {
            return fill[coord.x, coord.y, coord.z];
        }

        public void SetFill(Int3 coord, FILL_VALUE_TYPE value)
        {
            fill[coord.x, coord.y, coord.z] = value;
            isDirty = true;
        }
        
        public void PaintFill(Int3 coord, float strength)
        {
            //Debug.Log("Paint at coord:" + coord);
            fill[coord.x, coord.y, coord.z] =
                (byte)Mathf.Clamp(fill[coord.x, coord.y, coord.z] + 
                (int)(strength * fillMax), 
                fillMin, fillMax);
            isDirty = true;
        }

        public bool ContainsCell(Int3 coord)
        {
            return coord.x >= 0 && coord.x < dimension.x &&
                coord.y >= 0 && coord.y < dimension.y &&
                coord.z >= 0 && coord.z < dimension.z;
        }
    }
}