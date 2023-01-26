using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData 
{
    public static int[][] level = new int[][]
    {
        //1
        new int[] { 0 },
        new int[] { 0, 1 },
        new int[] { 0, 1, 8 },
        //2
        new int[] { 1, 2 },
        new int[] { 1, 2, 3 },
        new int[] { 1, 2, 3, 9 },

        new int[] { 2, 3 },
        new int[] { 2, 3, 4 },
        new int[] { 2, 3, 4, 10 },
        //3
        new int[] { 3, 4, 5 },
        new int[] { 3, 4, 5, 6 },
        new int[] { 3, 4, 5, 6, 11 },

        new int[] { 4, 5, 6 },
        new int[] { 4, 5, 6, 7 },
        new int[] { 4, 5, 6, 7, 12 },

        new int[] { 5, 6, 7 },
        new int[] { 5, 6, 7, 8 },
        new int[] { 5, 6, 7, 8, 13},
        //4
        new int[] { 6, 7, 8, 9 },
        new int[] { 6, 7, 8, 9, 10 },
        new int[] { 6, 7, 8, 9, 10, 14 },

        new int[] { 7, 8, 9, 10 },
        new int[] { 7, 8, 9, 10, 11 },
        new int[] { 7, 8, 9, 10, 11, 15 },

        new int[] { 8, 9, 10, 11 },
        new int[] { 8, 9, 10, 11, 12 },
        new int[] { 8, 9, 10, 11, 12, 16 },

        new int[] { 9, 10, 11, 12 },
        new int[] { 9, 10, 11, 12, 13 },
        new int[] { 9, 10, 11, 12, 13, 17 },
        //5
        new int[] { 10, 11, 12, 13, 14 },
        new int[] { 10, 11, 12, 13, 14, 15 },
        new int[] { 10, 11, 12, 13, 14, 15, 18 },

        new int[] { 11, 12, 13, 14, 15 },
        new int[] { 11, 12, 13, 14, 15, 16 },
        new int[] { 11, 12, 13, 14, 15, 16, 19 },

        new int[] { 12, 13, 14, 15, 16 },
        new int[] { 12, 13, 14, 15, 16, 17 },
        new int[] { 12, 13, 14, 15, 16, 17, 20 },

        new int[] { 13, 14, 15, 16, 17 },
        new int[] { 13, 14, 15, 16, 17, 18 },
        new int[] { 13, 14, 15, 16, 17, 18, 21 },

        new int[] { 14, 15, 16, 17, 18 },
        new int[] { 14, 15, 16, 17, 18, 19 },
        new int[] { 14, 15, 16, 17, 18, 19, 22 },
        //6
        new int[] { 15, 16, 17, 18, 19, 20 },
        new int[] { 15, 16, 17, 18, 19, 20, 21 },
        new int[] { 15, 16, 17, 18, 19, 20, 21, 23 },

        new int[] { 15, 16, 17, 18, 19, 20, 21, 22 },
        new int[] { 15, 16, 17, 18, 19, 20, 21, 22, 23 },
    };
}
