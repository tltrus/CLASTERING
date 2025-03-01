using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace GraphApp
{
    // Based on article: https://www.ocf.berkeley.edu/~fricke/projects/hoshenkopelman/hoshenkopelman.html
    class AlgHoshen_Kopelman
    {
        class Cell
        {
            public int x, y;
        }
        int[] labels;
        int inc_x = 0;
        int inc_y = 1;
        Cell active_cell;

        public AlgHoshen_Kopelman()
        {
            active_cell = new Cell();
        }
        public void CalculationStatic(int[,] matrix)
        {
            int rows = matrix.GetLength(0) - 1;
            int cols = matrix.GetLength(1) - 1;

            labels = new int[rows * cols / 2];

            for (int y = 1; y < rows; ++y)
            {
                for (int x = 1; x < cols; ++x)
                {
                    if (matrix[y, x] > 0)
                    {
                        int up = (y == 0 ? 0 : matrix[y - 1, x]);
                        int left = (x == 0 ? 0 : matrix[y, x - 1]);

                        if (left == 0 && up == 0)
                        {
                            labels[0]++;
                            labels[labels[0]] = labels[0];
                            matrix[y, x] = labels[0];
                        }
                        else
                        if (left > 0 && up == 0)            /* One neighbor, to the left. */
                        {
                            matrix[y, x] = Find(left);
                        }
                        else
                        if (left == 0 && up > 0)            /* One neighbor, above. */
                        {
                            matrix[y, x] = Find(up);
                        }
                        else                                /* Neighbors BOTH to the left and above. */
                        {
                            matrix[y, x] = Union(up, left);
                        }
                    }
                }
            }

            // Какая-то уловка, которая все делает как надо. Без кода ниже получается шляпа.

            int[] new_labels = new int[rows * cols / 2]; // allocate array, initialized to zero

            for (int i = 1; i < rows; i++)
                for (int j = 1; j < cols; j++)
                    if (matrix[i, j] > 0)
                    {
                        int x = Find(matrix[i, j]);
                        if (new_labels[x] == 0)
                        {
                            new_labels[0]++;
                            new_labels[x] = new_labels[0];
                        }
                        matrix[i, j] = new_labels[x];
                    }

            int total_clusters = new_labels[0];
        }
        private int Union(int x, int y)
        {
            var rez1 = Find(y);
            var rez2 = labels[Find(x)] = rez1;
            return rez2;

        }
        private int Find(int x)
        {
            int y = x;
            while (labels[y] != y)
                y = labels[y];
            while (labels[x] != x)
            {
                int z = labels[x];
                labels[x] = y;
                x = z;
            }
            return y;
        }
    }
}
