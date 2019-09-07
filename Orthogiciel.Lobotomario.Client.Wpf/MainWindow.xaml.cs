using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using Orthogiciel.Lobotomario.Core;
using System.IO;
using System.Drawing.Imaging;
using Orthogiciel.Lobotomario.Core.Properties;

namespace Orthogiciel.Lobotomario.Client.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            new Thread(() =>
            {
                while (true)
                {
                    TakeSnapshot();
                    Thread.Sleep(50); // Lower the delay for a faster refresh rate
                }
            }).Start();
        }

        private void TakeSnapshot()
        {
            var emulator = Process.GetProcessesByName("Mesen").FirstOrDefault();
            var winHandle = emulator.MainWindowHandle;
            var sc = new ScreenCapture();
            var screenshot = sc.CaptureWindow(winHandle);
            //var spriteFound = FindSprite(Properties.Resources.Block1, (Bitmap)screenshot);
            MarkSprites(Properties.Resources.Block1, (Bitmap)screenshot);

            Dispatcher.Invoke(() => this.ImgScreenshot.Source = ConvertToImageSource(screenshot));
        }

        private ImageSource ConvertToImageSource(System.Drawing.Image image)
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

        private bool FindSprite(Bitmap sprite, Bitmap image)
        {
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var found = true;

                    for (var x_sprite = 0; x_sprite < sprite.Width; x_sprite++)
                    {
                        for (var y_sprite = 0; y_sprite < sprite.Height; y_sprite++)
                        {
                            if (!PixelsMatch(sprite.GetPixel(x_sprite, y_sprite), image.GetPixel(x + x_sprite, y + y_sprite)))
                            {
                                found = false;
                                break;
                            }
                        }

                        if (!found)
                            break;
                    }

                    if (found)
                        return true;
                }
            }

            return false;
        }

        private void MarkSprites(Bitmap sprite, Bitmap image)
        {
            for (var x = 0; x < image.Width - 1; x++)
            {
                for (var y = 0; y < image.Height - 1; y++)
                {
                    var found = true;

                    for (var x_sprite = 1; x_sprite < sprite.Width - 1; x_sprite++)
                    {
                        for (var y_sprite = 1; y_sprite < sprite.Height - 1; y_sprite++)
                        {
                            if (!PixelsMatch(sprite.GetPixel(x_sprite, y_sprite), image.GetPixel(x + x_sprite, y + y_sprite)))
                            {
                                found = false;
                                break;
                            }
                        }

                        if (!found)
                            break;
                    }

                    if (found)
                    {
                        using (var g = Graphics.FromImage(image))
                        {
                            var rectangle = new System.Drawing.Rectangle(x - 1, y - 1, sprite.Width, sprite.Height);
                            g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Blue, 1f), rectangle);
                        }

                        y += sprite.Height - 1;
                    }
                }
            }
        }

        private bool PixelsMatch(System.Drawing.Color spritePixel, System.Drawing.Color imagePixel)
        {
            return spritePixel.A >= imagePixel.A - 5 && spritePixel.A <= imagePixel.A + 5 &&
                   spritePixel.R >= imagePixel.R - 5 && spritePixel.R <= imagePixel.R + 5 &&
                   spritePixel.G >= imagePixel.G - 5 && spritePixel.G <= imagePixel.G + 5 &&
                   spritePixel.B >= imagePixel.B - 5 && spritePixel.B <= imagePixel.B + 5;
        }
    }
}
