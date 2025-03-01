using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System;
using System.Linq;

namespace GraphApp
{
    public class Map
    {
        int rows, cols;
        public int[,] map;
        int size;
        Random rnd;
        int act_x, act_y;

        public Map(int width, int height, int cell_size)
        {
            size = cell_size;
            rows = height / cell_size;
            cols = width / cell_size;

            rnd = MainWindow.rnd;

            map = new int[rows, cols];

            for (int y = 1; y < rows - 1; y++)
                for (int x = 1; x < cols - 1; x++)
                {
                    var random = rnd.NextDouble();
                    if (random < 0.5)
                        map[y, x] = 1;
                }
        }

        public void SetActiveCell(int x, int y)
        {
            act_x = x;
            act_y = y;
        }

        public void Draw(DrawingVisual visual, DrawingContext dc)
        {
            for (int y = 0; y < rows; y++)
            {
                int offs_y = y * size;
                for (int x = 0; x < cols; x++)
                {
                    int offs_x = x * size;

                    Brush brush = Brushes.White;
                    Point textPos = new Point();

                    Rect rect = new Rect()
                    {
                        X = offs_x,
                        Y = offs_y,
                        Width = size,
                        Height = size
                    };

                    switch(map[y, x])
                    {
                        case 0:
                            brush = Brushes.White;
                            break;
                        case 1:
                            brush = Brushes.DarkGray;
                            break;
                        default:
                            // преобразуем Brushes в массив
                            var values = typeof(Brushes).GetProperties().Select(p => new { Name = p.Name, Brush = p.GetValue(null) as Brush }).ToArray();
                            brush = values[map[y, x]].Brush;
                            break;
                    }
                    if (map[y, x] == 1)
                        brush = Brushes.DarkGray;

                    Pen pen = new Pen(Brushes.Gray, 0.1);
                    dc.DrawRectangle(brush, pen, rect);

                    textPos = new Point(offs_x + 3, offs_y + 3);
                    var fontWeight = FontWeights.Normal;
                    if (act_x == x && act_y == y)
                    {
                        fontWeight = FontWeights.Bold;
                    }
                    FormattedText formattedText = new FormattedText(map[y, x].ToString(), CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black,
                            VisualTreeHelper.GetDpi(visual).PixelsPerDip);
                    formattedText.SetFontWeight(fontWeight);
                    dc.DrawText(formattedText, textPos);
                }
            }
        }
    }
}

