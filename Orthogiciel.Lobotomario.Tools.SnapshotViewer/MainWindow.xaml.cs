using Orthogiciel.Lobotomario.Core;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace Orthogiciel.Lobotomario.Tools.SnapshotViewer
{
    public partial class MainWindow : Window
    {
        private readonly Screen screen;

        private BackgroundWorker backgroundWorker;

        private bool isRunning = true;

        public MainWindow()
        {
            InitializeComponent();

            screen = new Screen("Mesen");
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            isRunning = false;
            base.OnClosing(e);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (isRunning)
            {
                Dispatcher.Invoke(() => 
                {
                    var imageSource = screen.TakeSnapshot();
                    imageViewer.Width = imageSource.Width;
                    imageViewer.Height = imageSource.Height;
                    imageViewer.Source = imageSource;

                    imageStats.Text = $"{imageSource.Width} x {imageSource.Height} ({imageSource.Width / imageSource.Height})";
                });

                Thread.Sleep(10);
            }
        }
    }
}
