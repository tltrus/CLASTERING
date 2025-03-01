using System.Collections.Generic;

namespace GraphApp
{
    class AlgBurning
    {
        class Cell
        {
            public int x, y;
        }
        Queue<Cell> queue = new Queue<Cell>();
        int label;
        int inc_x = 0;
        int inc_y = 1;
        Cell active_cell;

        public AlgBurning()
        {
            label = 1;
            active_cell = new Cell();
        }

        public (int,int) GetActiveCell() => (inc_x, inc_y);

        public void CalculationStatic(int[,] matrix)
        {
            int rows = matrix.GetLength(0) - 1;
            int cols = matrix.GetLength(1) - 1;

            for (int y = 1; y < rows; ++y)
            {
                for (int x = 1; x < cols; ++x)
                {
                    queue.Clear();

                    if (matrix[y, x] == 1)
                    {
                        label += 1;

                        matrix[y, x] = label;

                        int P = 1;
                        int next_x = x;
                        int next_y = y;
                        while (P > 0)
                        {
                            if (queue.Count > 0)
                            {
                                var next_cell = queue.Dequeue();
                                next_x = next_cell.x;
                                next_y = next_cell.y;
                            }
                            CheckNeighbours(matrix, next_x, next_y, label);

                            P = queue.Count;
                        }
                    }
                }
            }
        }
        public bool CalculationDynamic(int[,] matrix)
        {
            int rows = matrix.GetLength(0) - 1;
            int cols = matrix.GetLength(1) - 1;

            if (inc_x < cols)
            {
                inc_x++;
            }
            else
            {
                inc_y++;
                inc_x = 1;
            }

            if (inc_y >= rows) return true;

            active_cell.x = inc_x;
            active_cell.y = inc_y;

            queue.Clear();

            if (matrix[inc_y, inc_x] == 1)
            {
                label += 1;

                matrix[inc_y, inc_x] = label;

                int P = 1;
                int next_x = inc_x;
                int next_y = inc_y;
                while (P > 0)
                {
                    if (queue.Count > 0)
                    {
                        var next_cell = queue.Dequeue();
                        next_x = next_cell.x;
                        next_y = next_cell.y;
                    }
                    CheckNeighbours(matrix, next_x, next_y, label);

                    P = queue.Count;
                }
            }

            return false;
        }

        void CheckNeighbours(int[,] matrix, int x, int y, int label)
        {
            // check arround
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    if (i == 0 && j == 0 || (i + j) == 0 || (i + j) == 2 || (i + j) == -2) continue;

                    int yn = y + i;
                    int xn = x + j;

                    if (matrix[yn, xn] == 1)
                    {
                        queue.Enqueue(new Cell() { x = xn, y = yn });
                        matrix[yn, xn] = label;
                    }
                }
            }
        }
    }
}
