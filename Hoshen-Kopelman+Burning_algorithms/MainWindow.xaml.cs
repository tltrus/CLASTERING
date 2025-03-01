using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace GraphApp
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        public static Random rnd = new Random();
        Map Field;
        DrawingVisual visual;
        DrawingContext dc;
        int width, height;
        AlgBurning Burning;
        AlgHoshen_Kopelman Hoshen_Kopelman;

        public MainWindow()
        {
            InitializeComponent();

            width = (int)g.Width;
            height = (int)g.Height;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            visual = new DrawingVisual();
            Init();
            Draw();
        }

        private void Init()
        {
            Field = new Map(width, height, 19);
            Burning = new AlgBurning();
            Hoshen_Kopelman = new AlgHoshen_Kopelman();
        }

        private void Draw()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                (int x, int y) = Burning.GetActiveCell();

                Field.SetActiveCell(x, y);
                Field.Draw(visual, dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            bool isDone = Burning.CalculationDynamic(Field.map);
            Draw();

            if (isDone) timer.Stop();
        }

        private void btnBurningUpdate_Click(object sender, RoutedEventArgs e)
        {
            Burning.CalculationStatic(Field.map);
            Draw();
        }
        private void btnBurningDynamic_Click(object sender, RoutedEventArgs e) => timer.Start();

        private void btnHoshKopUpdate_Click(object sender, RoutedEventArgs e)
        {
            Hoshen_Kopelman.CalculationStatic(Field.map);
            Draw();
        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Draw();
        }
    }
}
