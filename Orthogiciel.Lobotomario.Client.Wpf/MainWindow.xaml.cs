using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Orthogiciel.Lobotomario.Core;

namespace Orthogiciel.Lobotomario.Client.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Screen Screen { get; private set; }

        public BackgroundWorker Worker { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Screen = new Screen("Mesen");
            this.Worker = new BackgroundWorker();
            //this.Worker.WorkerReportsProgress = true;
            this.Worker.DoWork += Worker_DoWork;
            //this.Worker.ProgressChanged += Worker_ProgressChanged;
            this.Worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //var snapshot = this.Screen.GetSnapshot();
                //this.Worker.ReportProgress(0, this.Screen.GetSnapshot());  
                Dispatcher.Invoke(() => this.ImgScreenshot.Source = this.Screen.GetSnapshot());
                Thread.Sleep(100); // Lower the delay for a faster refresh rate
            }
        }

        //private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    Dispatcher.Invoke(() => this.ImgScreenshot.Source = (ImageSource)e.UserState);
        //}
    }
}
