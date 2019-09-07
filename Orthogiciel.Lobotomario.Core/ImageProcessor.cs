using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Orthogiciel.Lobotomario.Core
{
    public static class ImageProcessor
    {
        public static void MarkSprites(Bitmap sprite, Bitmap image)
        {
            // TODO : gérer ça en plusieurs étapes
            //        1. Trouver une tuile
            //        2. Ensuite, rechercher les tuiles en faisant des sauts à partir de celle là

            for (var x = 0; x < image.Width - 1; x++)
            {
                var matchFoundInColumn = false;

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

                        matchFoundInColumn = true;
                    }
                }

                if (matchFoundInColumn)
                    x += sprite.Width - 1;
            }
        }

        public static bool PixelsMatch(System.Drawing.Color spritePixel, System.Drawing.Color imagePixel)
        {
            return spritePixel.A >= imagePixel.A - 5 && spritePixel.A <= imagePixel.A + 5 &&
                   spritePixel.R >= imagePixel.R - 5 && spritePixel.R <= imagePixel.R + 5 &&
                   spritePixel.G >= imagePixel.G - 5 && spritePixel.G <= imagePixel.G + 5 &&
                   spritePixel.B >= imagePixel.B - 5 && spritePixel.B <= imagePixel.B + 5;
        }
    }
}
