using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsoSurface {
    public class SurfaceNetTestor : MonoBehaviour {
        SurfaceNetSolver solver;
        IsoGrid grid;

        public bool smoothGrid = false;

        // Use this for initialization
        void Start() {
            grid = GetComponent<IsoGrid>();
            solver = GetComponent<SurfaceNetSolver>();
            Test();
        }

        [ContextMenu("Test")]
        public void Test()
        {
            if (smoothGrid)
                grid.GenerateSmooothSphere();
            else
                grid.GenerateSphere();
            solver.GenerateLookups();
            solver.Solve();
            solver.Show();
        }
        
    }
}