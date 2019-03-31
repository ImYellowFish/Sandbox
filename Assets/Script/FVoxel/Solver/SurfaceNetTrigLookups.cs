using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel
{
    public class SurfaceNetTrigLookups
    {
        private static SurfaceNetTrigLookups _instance;
        public static SurfaceNetTrigLookups Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SurfaceNetTrigLookups();
                return _instance;
            }
        }

        // Lookups:
        // 8 cell vertices position arranged with vertex indices in 0~7
        public Vector3[] vertexPosLookup;
        // List of edge interection points. [edgeVert0A, edgeVert0B, edgeVert1A, edgeVert1B...]
        public List<int>[] intersectionVertLookup;
        // List of edge intersection count along each of the 3 axis
        public int[,] intersectionAxisLookup;

        public readonly Int3[] cellOffsetsLookup = new Int3[]
        {
            new Int3(0,0,0),
            new Int3(1,0,0),
            new Int3(1,0,1),
            new Int3(0,0,1),
            new Int3(0,1,0),
            new Int3(1,1,0),
            new Int3(1,1,1),
            new Int3(0,1,1),
        };

        public SurfaceNetTrigLookups()
        {
            Generate();
        }

        /// <summary>
        /// Generate all lookups
        /// </summary>
        public void Generate()
        {
            vertexPosLookup = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                int xi = (i >> 1 & 1) ^ (i & 1);
                int yi = (i >> 2 & 1);
                int zi = (i >> 1 & 1);
                vertexPosLookup[i] = new Vector3(xi, yi, zi);
            }

            int[] cellDepths = new int[8];
            intersectionVertLookup = new List<int>[256];
            intersectionAxisLookup = new int[256, 3];
            for (int mask = 0; mask <= 0xFF; mask++)
            {
                for (int i = 0; i < 8; i++)
                    cellDepths[i] = mask >> i & 1;
                intersectionVertLookup[mask] = new List<int>();
                intersectionAxisLookup[mask, 0] = 0;
                intersectionAxisLookup[mask, 1] = 0;
                intersectionAxisLookup[mask, 2] = 0;
                for (int edge = 0; edge < 12; edge++)
                {
                    int vert0, vert1;
                    if (edge >= 8)
                    {
                        vert0 = edge - 8;
                        vert1 = edge - 4;
                    }
                    else if (edge >= 4)
                    {
                        vert0 = edge;
                        vert1 = (edge - 3) % 4 + 4;
                    }
                    else
                    {
                        vert0 = edge;
                        vert1 = (edge + 1) % 4;
                    }
                    int depth0 = cellDepths[vert0];
                    int depth1 = cellDepths[vert1];
                    if (depth0 != depth1)
                    {
                        intersectionVertLookup[mask].Add(vert0);
                        intersectionVertLookup[mask].Add(vert1);

                        var dirVec = vertexPosLookup[vert0] - vertexPosLookup[vert1];
                        int dirIndex = Mathf.RoundToInt(Mathf.Abs(dirVec[1])) * 1 + Mathf.RoundToInt(Mathf.Abs(dirVec[2])) * 2;
                        intersectionAxisLookup[mask, dirIndex]++;
                    }
                }
                //Debug.Log("mask:" + mask + "|| " + "intersection count:" + Utility.IterableToString(intersectionAxisLookup, mask));
                //Debug.Log("mask:" + mask + "|| " + Utility.IterableToString(intersectionVertLookup[mask]));
            }
        }
    }
}
