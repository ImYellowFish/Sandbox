using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveBlock {
    public class NaiveBlockSmoother : MonoBehaviour {
        public NaiveBlockTrunk trunk;
        public Cube[,,] cubes;
        public int trunkSize { get { return trunk.trunkSize; } }
        public int trunkHeight { get { return trunk.trunkHeight; } }

        // Use this for initialization
        void Start() {
            trunk = GetComponent<NaiveBlockTrunk>();
            cubes = trunk.cubes;
        }
        
        [ContextMenu("Process")]
        public void Process()
        {
            cubes = trunk.cubes;

            for(int i = 0; i < trunkSize; i++)
            {
                for(int j = 0; j < trunkSize; j++)
                {
                    for(int k = 0; k < trunkHeight; k++)
                    {
                        ProcessCube(i, j, k);
                    }
                }
            }
        }

        public void ProcessCube(int i, int j, int k)
        {
            if (!IsTransparent(i, j, k))
            {
                return;
            }

        }

        private bool IsTransparent(int i, int j, int k)
        {
            if(IsInsideCube(i,j,k))
            {
                return cubes[i, j, k].fill == 0;
            }
            return false;
        }

        private bool IsInsideCube(int i, int j, int k)
        {
            return i >= 0 && i < trunkSize && j >= 0 && j < trunkSize && k >= 0 && k < trunkHeight;
        }
    }
}