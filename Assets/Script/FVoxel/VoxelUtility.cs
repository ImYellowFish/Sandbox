using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FVoxel
{
    [System.Serializable]
    public struct Int3
    {
        public int x;
        public int y;
        public int z;

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Int3 Clone()
        {
            return new Int3(x, y, z);
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid Int3 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid Int3 index!");
                }
            }
        }

        public static Int3 operator +(Int3 a, Int3 b)
        {
            return new Int3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Int3 operator -(Int3 a, Int3 b)
        {
            return new Int3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public Int3 Offset(int dimension, int offsetValue)
        {
            Int3 result = Clone();
            result[dimension] += offsetValue;
            return result;
        }

        public Int3 Offset(int dx, int dy, int dz)
        {
            return new Int3(x + dx, y + dy, z + dz);
        }

        public static Int3 Zero
        {
            get { return new Int3(0, 0, 0); }
        }

        public Int3 Clamp(Int3 min, Int3 max)
        {
            return new Int3(Mathf.Clamp(x, min.x, max.x),
                Mathf.Clamp(y, min.y, max.y),
                Mathf.Clamp(z, min.z, max.z));
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }
    }

    public class SwapBuffer2D
    {

    }
}