using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Assume that each cell has a size of 2

// Square vertices setup:
//
// (0,2), (1,2), (2,2)
// (0,1), (1,1), (2,1)
// (0,0), (1,0), (2,0)
//
// vertice coord to dictionary key:
// key(x, y, cell_x, cell_y) = cell_y * cells_per_row << 4 + cell_x << 4 + y << 2 + x

// (assume that cells_per_row = 2^w)
// key(x, y, cell_x, cell_y) = cell_y << (w + 4) + cell_x << 4 + y << 2 + x

// Reference:
// https://en.wikipedia.org/wiki/Marching_squares
namespace MarchingSquare
{
    public static class MarchingSquareData
    {
        /// <summary>
        /// Local coordinates for each cell vertex.
        /// cell vertex index is arranged in anti-clockwise order.
        /// 
        /// 3 ----- 2
        /// ---------
        /// 0 ----- 1
        /// 
        /// </summary>
        public static int[][] cellVertPos = new int[][]
        {
            new int[2] {0,0},
            new int[2] {2,0},
            new int[2] {2,2},
            new int[2] {0,2},
        };


        /// <summary>
        /// Contour vert points for each case.
        /// Verts for each case is an int array of (x,y) with a length of vertCount * 2.
        /// Arranged in clockwise order.
        /// </summary>
        public static int[][] vertices = new int[][]
        {
            new int[0] {},
            new int[6] {0,0,0,1,1,0,},
            new int[6] {1,0,2,1,2,0,},
            new int[8] {0,0,2,1,2,0,0,1,},
            new int[6] {2,1,1,2,2,2,},
            new int[12] {0,0,0,1,1,0,2,1,1,2,2,2,},
            new int[8] {2,0,1,0,1,2,2,2,},
            new int[10] {2,0,0,0,0,1,1,2,2,2,},
            new int[6] {0,1,0,2,1,2,},
            new int[8] {0,0,0,2,1,0,1,2,},
            new int[12] {2,0,1,0,2,1,0,1,1,2,0,2,},
            new int[10] {0,0,2,1,2,0,1,2,0,2,},
            new int[8] {0,1,0,2,2,1,2,2,},
            new int[10] {0,0,0,2,1,0,2,1,2,2,},
            new int[10] {0,1,0,2,2,2,1,0,2,0,},
            new int[8] {0,0,2,2,2,0,0,2,},
        };

        /// <summary>
        /// Contour triangle points for each case.
        /// </summary>
        public static int[][] triangles = new int[][]
        {
            new int[0] {},
            new int[3] {0,1,2,},
            new int[3] {0,1,2,},
            new int[6] {0,1,2,0,3,1,},
            new int[3] {0,1,2,},
            new int[12] {0,1,2,2,1,3,1,4,3,3,4,5,},
            new int[6] {0,1,2,0,2,3,},
            new int[9] {0,1,2,0,2,3,0,3,4,},
            new int[3] {0,1,2,},
            new int[6] {0,1,2,2,1,3,},
            new int[12] {0,1,2,1,3,2,2,3,4,3,5,4,},
            new int[9] {0,1,2,0,3,1,0,4,3,},
            new int[6] {0,1,2,2,1,3,},
            new int[9] {0,1,2,2,1,3,3,1,4,},
            new int[9] {0,1,2,3,0,2,4,3,2,},
            new int[6] {0,1,2,0,3,1,},
        };

        /// <summary>
        /// Interpolation anchors for each vertex.
        /// Each vertex has zero or two anchors.
        /// 
        /// Each anchor list is arranged as a list of (anchorIndexA, anchorIndexB),
        /// with a length of vertCount.
        /// 
        /// anchorIndex ranges from 0 ~ 3, corresponding to 4 cell vertices with anti-clockwise order.
        /// anchorIndexA has a smaller cell value(0), while parentCellIndexB has a bigger cell value(>0).
        /// </summary>
        public static int[][][] anchors = new int[][][]
        {
            new int[0][] {},
            new int[3][] {null,new int[2]{3,0},new int[2]{1,0},},
            new int[3][] {new int[2]{0,1},new int[2]{2,1},null,},
            new int[4][] {null,new int[2]{2,1},null,new int[2]{3,0},},
            new int[3][] {new int[2]{1,2},new int[2]{3,2},null,},
            new int[6][] {null,new int[2]{3,0},new int[2]{1,0},new int[2]{1,2},new int[2]{3,2},null,},
            new int[4][] {null,new int[2]{0,1},new int[2]{3,2},null,},
            new int[5][] {null,null,new int[2]{3,0},new int[2]{3,2},null,},
            new int[3][] {new int[2]{0,3},null,new int[2]{2,3},},
            new int[4][] {null,null,new int[2]{1,0},new int[2]{2,3},},
            new int[6][] {null,new int[2]{0,1},new int[2]{2,1},new int[2]{0,3},new int[2]{2,3},null,},
            new int[5][] {null,new int[2]{2,1},null,new int[2]{2,3},null,},
            new int[4][] {new int[2]{0,3},null,new int[2]{1,2},null,},
            new int[5][] {null,null,new int[2]{1,0},new int[2]{1,2},null,},
            new int[5][] {new int[2]{0,3},null,null,new int[2]{0,1},null,},
            new int[4][] {null,null,null,null,},
        };
    }

}
