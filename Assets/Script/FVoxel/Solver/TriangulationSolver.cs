using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel
{
    public abstract class TriangulationSolver
    {
        public TriangulationSolver(VoxelTrunk trunk)
        {
            this.trunk = trunk;
            data = trunk.data;
        }

        public abstract void Solve(Mesh mesh);
        public VoxelTrunk trunk;
        public VoxelData data;
    }
}