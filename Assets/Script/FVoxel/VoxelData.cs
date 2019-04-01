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
        /// The trunk this data belongs to.
        /// </summary>
        public VoxelTrunk trunk;

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

        public VoxelData(VoxelTrunk trunk, Int3 dimension, Vector3 cellSize)
        {
            this.trunk = trunk;
            this.dimension = dimension;
            this.cellSize = cellSize;
            fill = new byte[dimension.x, dimension.y, dimension.z];
        }

        public bool ContainsCell(Int3 coord)
        {
            return coord >= Int3.Zero && coord < dimension;
        }

        public FILL_VALUE_TYPE GetFill(Int3 coord)
        {
            try
            {
                return fill[coord.x, coord.y, coord.z];
            }
            catch (System.IndexOutOfRangeException e)
            {
                Debug.LogError("Coordinate is not inside trunk data:" + coord);
                throw e;
            }
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

        /// <summary>
        /// Get the fill value of a cell which belongs to neighboring trunks.
        /// </summary>
        public FILL_VALUE_TYPE GetCrossBoundaryFill(Int3 coord)
        {
            VoxelTrunk otherTrunk;
            var otherCoord = trunk.GetCrossBoundaryCellInfo(coord, out otherTrunk);
            if (otherTrunk != null)
            {
                //Debug.Log("Coord:" + coord + ", OtherCoord:" + otherCoord + ", Value:" + otherTrunk.data.GetFill(otherCoord));
                //Debug.Log("Trunk:" + trunk.coordinate + ", otherTrunk:" + otherTrunk.coordinate);
                return otherTrunk.data.GetFill(otherCoord);
            }
            else
            {
                return FILL_VALUE_TYPE.MinValue;
            }
        }
    }
}