﻿using System.Collections.Generic;

 namespace Core.Helpers
{
    public static class StringMatrixHelper
    {
        public static List<List<string>> Transpose(this List<List<string>> inputMatrix)
        {
            List<List<string>> retsultlist = new List<List<string>>();
            if (inputMatrix.Count <= 0)
                return retsultlist;

            int rows = inputMatrix.Count;
            int cols = inputMatrix[0].Count;
            if (cols == 0)
                return retsultlist;

            for (int i = 0; i < cols; i++)
            {
                List<string> tmplist = new List<string>();
                for (int j = 0; j < rows; j++)
                {
                    tmplist.Add(inputMatrix[j][i]);
                }

                retsultlist.Add(tmplist);
            }

            return retsultlist;
        }
    }
}