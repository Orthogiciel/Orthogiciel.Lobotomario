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
        public static void MarkSprites(Tile tile, Bitmap image)
        {
            var tileSet = Tile.Tileset;
            var currentDateDate = DateTime.Now;
            
            // TODO : gérer ça en plusieurs étapes
            //        1. Trouver une tuile
            //        2. Ensuite, rechercher les tuiles en faisant des sauts à partir de celle là
            
            for (var x = 1; x < image.Width - 1; x++)
            {
                var matchFoundInColumn = false;

                for (var y = 1; y < image.Height - 1; y++)
                {
                    for (var i = 0; i < tile.TilesetPositions.Count; i++)
                    {
                        var pos = tile.TilesetPositions[i];
                        var found = true;

                        for (var x_sprite = pos.X + 1; x_sprite < pos.X + tile.Width - 1; x_sprite++)
                        {
                            for (var y_sprite = pos.Y + 1; y_sprite < pos.Y + tile.Height - 1; y_sprite++)
                            { 
                                if (!PixelsMatch(tileSet.GetPixel(x_sprite, y_sprite), image.GetPixel(x + x_sprite - pos.X, y + y_sprite - pos.Y)))
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
                                var rectangle = new System.Drawing.Rectangle(x - 1, y - 1, tile.Width, tile.Height);
                                g.DrawRectangle(new System.Drawing.Pen(tile.MarkColor, 1f), rectangle);
                            }

                            y += tile.Height - 1;

                            matchFoundInColumn = true;
                            i = tile.TilesetPositions.Count;

                            currentDateDate = DateTime.Now;
                        }
                    }     
                }

                if (matchFoundInColumn)
                    x += tile.Width - 1;
            }         
        }

        public static bool PixelsMatch(System.Drawing.Color spritePixel, System.Drawing.Color imagePixel)
        {
            return spritePixel.A >= imagePixel.A - 5 && spritePixel.A <= imagePixel.A + 5 &&
                   spritePixel.R >= imagePixel.R - 40 && spritePixel.R <= imagePixel.R + 40 &&
                   spritePixel.G >= imagePixel.G - 5 && spritePixel.G <= imagePixel.G + 5 &&
                   spritePixel.B >= imagePixel.B - 5 && spritePixel.B <= imagePixel.B + 5;
        }
    }
}
