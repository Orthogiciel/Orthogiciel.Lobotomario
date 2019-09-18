using Orthogiciel.Lobotomario.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Orthogiciel.Lobotomario.Tools.SnapshotViewer
{
    public partial class MainWindow : Window
    {
        private readonly SnapshotEngine engine;

        public MainWindow()
        {
            DataContext = this;
            engine = new SnapshotEngine();
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    engine.Updated += Engine_Updated;
                    engine.Start();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            base.OnInitialized(e);
        }

        private void Engine_Updated(object sender, Image e)
        {
            Dispatcher.Invoke(() =>
            {
                imageViewer.Width = e.Width;
                imageViewer.Height = e.Height;
                imageViewer.Source = ConvertToImageSource(e);
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            engine.Stop();
            base.OnClosing(e);
        }

        private ImageSource ConvertToImageSource(Image image)
        {
            var bitmap = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Jpeg);
                stream.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }
    }
}
