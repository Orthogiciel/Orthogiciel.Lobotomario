using Orthogiciel.Lobotomario.Core;
using System;
using System.Windows;

namespace Orthogiciel.Lobotomario.Tools.SnapshotViewer
{
    public partial class MainWindow : Window
    {
        private readonly Engine engine;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            engine = new Engine();
        }

        protected override void OnInitialized(EventArgs e)
        {
            try
            {
                engine.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            base.OnInitialized(e);
        }
    }
}
