using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock
{
    public static class NaiveBlockData
    {
        public static List<Vector3> baseCubeVertices = new List<Vector3>()
        {
            new Vector3(0,0,0),
            new Vector3(0,1,0),
            new Vector3(1,1,0),
            new Vector3(1,0,0),

            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(1,0,1),
            new Vector3(0,0,1),

            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(0,1,1),
            new Vector3(0,1,0),

            new Vector3(1,0,0),
            new Vector3(1,1,0),
            new Vector3(1,1,1),
            new Vector3(1,0,1),

            new Vector3(0,1,0),
            new Vector3(0,1,1),
            new Vector3(1,1,1),
            new Vector3(1,1,0),

            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(1,1,1),
            new Vector3(0,1,1),
        };

        public static List<Vector2> baseCubeUVs = new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),

            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1),

            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1),

            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),

            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1),
            new Vector2(0,0),

            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
        };

        public static List<int> baseCubeTriangles = new List<int>()
        {
            0,1,2,
            0,2,3,

            4,5,6,
            4,6,7,

            8,9,10,
            8,10,11,

            12,13,14,
            12,14,15,

            16,17,18,
            16,18,19,

            20,21,22,
            20,22,23,
        };

        public static List<int> quadTriangles = new List<int>()
        {
            0, 1, 2,
            0, 2, 3,
        };
    }
}