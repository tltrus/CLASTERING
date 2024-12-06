using System.Windows;
using ScottPlot;

namespace WpfApp
{

    public partial class MainWindow : Window
    {
        double[][] rawData = new double[20][];
        int[] clustering;
        Kmeans kmeans;
        int numClusters = 3;
        IPalette palette;
        IEnumerable<int> clusters;

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }


        private void Init()
        {
            kmeans = new Kmeans();

            // data init
            rawData[0] = new double[] { 65.0, 220.0 };
            rawData[1] = new double[] { 73.0, 160.0 };
            rawData[2] = new double[] { 59.0, 110.0 };
            rawData[3] = new double[] { 61.0, 120.0 };
            rawData[4] = new double[] { 75.0, 150.0 };
            rawData[5] = new double[] { 67.0, 240.0 };
            rawData[6] = new double[] { 68.0, 230.0 };
            rawData[7] = new double[] { 70.0, 220.0 };
            rawData[8] = new double[] { 62.0, 130.0 };
            rawData[9] = new double[] { 66.0, 210.0 };
            rawData[10] = new double[] { 77.0, 190.0 };
            rawData[11] = new double[] { 75.0, 180.0 };
            rawData[12] = new double[] { 74.0, 170.0 };
            rawData[13] = new double[] { 70.0, 210.0 };
            rawData[14] = new double[] { 61.0, 110.0 };
            rawData[15] = new double[] { 58.0, 100.0 };
            rawData[16] = new double[] { 66.0, 230.0 };
            rawData[17] = new double[] { 59.0, 120.0 };
            rawData[18] = new double[] { 68.0, 210.0 };
            rawData[19] = new double[] { 61.0, 130.0 };

            clusters = null;

            // ScotPlot draw
            WpfPlot1.Plot.Clear();

            palette = new ScottPlot.Palettes.Category10();

            for (int i = 0; i < rawData.Length; ++i)
            {
                var x = rawData[i][0]; // X
                var y = rawData[i][1]; // Y
                var c = WpfPlot1.Plot.Add.Circle(x, y, .9);

                c.FillStyle.Color = Colors.Blue;
                c.LineWidth = 0;
            }

            // force circles to remain circles
            ScottPlot.AxisRules.SquareZoomOut squareRule = new(WpfPlot1.Plot.Axes.Bottom, WpfPlot1.Plot.Axes.Left);
            WpfPlot1.Plot.Axes.Rules.Add(squareRule);

            WpfPlot1.Refresh();
        }


        private void btnClustering_Click(object sender, RoutedEventArgs e)
        {
            // get clustering
            clustering = kmeans.Cluster(rawData, numClusters, 0);

            clusters = clustering
                            .GroupBy(item => item)
                            .Select(grp => grp.Key);

            WpfPlot1.Plot.Clear();

            for (int i = 0; i < clustering.Length; ++i)
            {
                if (clusters.Contains(clustering[i]))
                {
                    var x = rawData[i][0]; // X
                    var y = rawData[i][1]; // Y
                    var c = WpfPlot1.Plot.Add.Circle(x, y, .9);

                    c.FillStyle.Color = palette.GetColor(clustering[i]);
                    c.LineWidth = 0;
                }
            }

            WpfPlot1.Refresh();
        }

        private void btnCheckOutlier_Click(object sender, RoutedEventArgs e)
        {
            if (clustering is null || clusters is null) return;

            
            for (int i = 0; i < clusters.Count(); ++i)
            {
                int outlier_index = kmeans.Outlier_index(rawData, clustering, clusters.Count(), i);

                var x = rawData[outlier_index][0]; // X
                var y = rawData[outlier_index][1]; // Y
                var c = WpfPlot1.Plot.Add.Circle(x, y, 3.0);

                c.FillStyle.Color = palette.GetColor(clustering[outlier_index]);
                c.LineWidth = 0;
            }

            WpfPlot1.Refresh();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Init();
        }
    }
}