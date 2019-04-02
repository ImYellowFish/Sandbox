using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FILL_VALUE_TYPE = System.Byte;

namespace FVoxel {
    [System.Serializable]
    public class VoxelData {
        
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

        #region Dirty region

        /// <summary>
        /// Whether the grid data has changed since last triangulation
        /// </summary>
        public bool isDirty { get { return dirtyRegionMin <= dirtyRegionMax; } }

        public Int3 dirtyRegionMin = Int3.Zero;
        public Int3 dirtyRegionMax = new Int3(-1,-1,-1);

        public void ClearDirty()
        {
            dirtyRegionMin = dimension;
            dirtyRegionMax = new Int3(-1, -1, -1);
        }

        public void SetDirty(Int3 coord)
        {
            dirtyRegionMin = Int3.Min(coord, dirtyRegionMin);
            dirtyRegionMax = Int3.Max(coord, dirtyRegionMax);
        }

        public void SetAllDirty()
        {
            dirtyRegionMin = Int3.Zero;
            dirtyRegionMax = dimension.Offset(-1,-1,-1);
        }

        #endregion

        #region Fill

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
            SetDirty(coord);
        }
        
        public void PaintFill(Int3 coord, float strength)
        {
            //Debug.Log("Paint at coord:" + coord);
            fill[coord.x, coord.y, coord.z] =
                (byte)Mathf.Clamp(fill[coord.x, coord.y, coord.z] + 
                (int)(strength * fillMax), 
                fillMin, fillMax);
            SetDirty(coord);
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

        #endregion
    }
}