﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Orthogiciel.Lobotomario.Core
{
    public class Screen
    {
        private readonly Process emulatorProcess;

        public Screen(Process emulatorProcess)
        {
            this.emulatorProcess = emulatorProcess;
        }

        public Bitmap TakeSnapshot()
        {
            return (Bitmap)CaptureGameScreen(emulatorProcess.MainWindowHandle);
        }

        public ImageSource ConvertToImageSource(Image image)
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

        public Image CaptureGameScreen(IntPtr handle)
        {
            int windowTopOffset = 55;
            IntPtr hdcSrc = User32.GetWindowDC(handle);

            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);

            int width = windowRect.right - windowRect.left - 20;
            int height = windowRect.bottom - windowRect.top - windowTopOffset - 8;

            int diffx = 0;
            int offsetx = 0;

            int diffy = 0;
            int offsety = 0;

            if (height / width < 0.9375)
            {
                var idealWidth = (int)(height * 1.0667);
                diffx = width - idealWidth > 0 ? width - idealWidth : 0;
                offsetx = diffx / 2;
            }
            else if (height / width > 0.9375)
            {
                var idealHeight = (int)(width * 0.9375);
                diffy = height - idealHeight > 0 ? height - idealHeight : 0;
                offsety = diffy / 2;
            }

            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width - diffx, height - diffy);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            
            GDI32.BitBlt(hdcDest, 0, 0, width - offsetx, height - offsety, hdcSrc, 10 + offsetx, windowTopOffset + offsety, GDI32.SRCCOPY);

            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            Debug.Print(img.HorizontalResolution.ToString());
            Debug.Print(img.VerticalResolution.ToString());
            Debug.Print(img.Width.ToString());
            Debug.Print(img.Height.ToString());
            GDI32.DeleteObject(hBitmap);

            return ResizeImage(img, 256, 240);
        }

        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }
    }
}
