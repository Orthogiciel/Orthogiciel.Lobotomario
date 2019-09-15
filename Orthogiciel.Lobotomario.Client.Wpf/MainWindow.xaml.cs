using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orthogiciel.Lobotomario.Core;
using Orthogiciel.Lobotomario.Core.GameObjects;

namespace Orthogiciel.Lobotomario.Client.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly Engine engine;

        public MainWindow()
        {
            DataContext = this;
            engine = new Engine();

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            try
            {
                engine.Updated += Engine_Updated;
                engine.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            base.OnInitialized(e);
        }

        private void Engine_Updated(object sender, GameState e)
        {
            var gameStateView = new Bitmap(256, 240);

            using (var g = Graphics.FromImage(gameStateView))
            {
                // Dessin des tuiles
                e.CurrentState.Where(o => o.GetType() == typeof(Tile)).ToList().ForEach(t =>
                {
                    g.FillRectangle(new SolidBrush(t.MarkColor), t.Bounds);
                });

                // Dessin du joueur
                var player = e.CurrentState.Where(o => o.GetType() == typeof(Mario)).FirstOrDefault();

                if (player != null)
                {
                    g.FillRectangle(new SolidBrush(player.MarkColor), player.Bounds);
                }
            }

            Dispatcher.Invoke(() => this.ImgScreenshot.Source = ConvertToImageSource(gameStateView));
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
