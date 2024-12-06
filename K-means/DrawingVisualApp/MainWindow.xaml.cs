using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawingVisualApp
{
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        int width, height;

        DrawingVisual visual;
        DrawingContext dc;
        Point mouse;

        class Point2D
        {
            public Point pos;
            public int claster = -1;
            public Brush brush;

            public Point2D(Point pos)
            {
                this.pos = pos;
                brush = Brushes.White;
            }
            public void SetColor(Brush brush) => this.brush = brush;
        }

        List<Point2D> points = new List<Point2D>();
        List<Point2D> centroids = new List<Point2D>();

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            Init();
        }

        void Init()
        {
            for (int i = 0; i < 30; i++)
            {
                var p = new Point(rnd.Next(width), rnd.Next(height));
                var point = new Point2D(p);
                points.Add(point);
            }
            Drawing();
        }

        private void Clastering()
        {
            UpdateClasters();
            RecalcCentroidPos();
        }
        private void RecalcCentroidPos()
        {
            double x = 0;
            double y = 0;

            for (int i = 0; i < centroids.Count; ++i)
            {
                var minX = double.MaxValue;
                var maxX = double.MinValue;
                var minY = double.MaxValue;
                var maxY = double.MinValue;
                foreach (var p in points)
                {
                    if (p.claster == i)
                    {
                        if (p.pos.X < minX) minX = p.pos.X;
                        if (p.pos.X > maxX) maxX = p.pos.X;
                        if (p.pos.Y < minY) minY = p.pos.Y;
                        if (p.pos.Y > maxY) maxY = p.pos.Y;

                        x = (maxX - minX) / 2 + minX;
                        y = (maxY - minY) / 2 + minY;
                    }
                }
                centroids[i].pos = new Point(x, y);
            }
        }
        private void UpdateClasters()
        {
            foreach (var p in points)
            {
                var minLen = double.MaxValue;
                var minC = 0; // индекс ближайшего центроида
                foreach (var c in centroids)
                {
                    var dist = Dist(p.pos, c.pos);
                    if (dist < minLen)
                    {
                        minLen = dist;
                        minC = centroids.IndexOf(c);
                        p.claster = minC;
                        p.brush = c.brush;
                    }
                }
            }
        }
        private double Dist(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                foreach (var p in points)
                {
                    dc.DrawEllipse(p.brush, null, p.pos, 3, 3);

                    if (p.claster > -1)
                    {
                        int index = p.claster;
                        dc.DrawLine(new Pen(Brushes.White, 0.3), p.pos, centroids[index].pos);
                    }
                }
                foreach (var c in centroids)
                {
                    dc.DrawEllipse(c.brush, null, c.pos, 8, 8);
                }
                dc.Close();
                g.AddVisual(visual);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count <= 0 || centroids.Count <= 0) return;
            Clastering();
            Drawing();
        }

        private void g_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouse = e.GetPosition(g);
            points.Add(new Point2D(mouse));
            Drawing();
        }

        private void g_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouse = e.GetPosition(g);
            var c = new Point2D(mouse);
            var brush = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255)));
            c.SetColor(brush);
            centroids.Add(c);
            Drawing();
        }
    }
}
