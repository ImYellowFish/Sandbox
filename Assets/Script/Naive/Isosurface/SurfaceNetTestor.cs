using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveIsoSurface {
    public class SurfaceNetTestor : MonoBehaviour {
        NaiveSurfaceNetSolver solver;
        IsoGrid grid;

        public bool smoothGrid = false;

        // Use this for initialization
        void Start() {
            grid = GetComponent<IsoGrid>();
            solver = GetComponent<NaiveSurfaceNetSolver>();
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