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
        public static void MarkTiles(Bitmap snapshot, bool drawGrid)
        {
            var firstTilePosition = FindFirstTile(snapshot);

            if (firstTilePosition != null)
            {
                if (drawGrid)
                    DrawGrid(snapshot, firstTilePosition.Value);

                var tileset = Tile.Tileset;                
                var offsetX = firstTilePosition.Value.X % 16;
                var offsetY = firstTilePosition.Value.Y % 16;                

                for (var idx = 0; idx < GameObjects.Tiles.Count; idx++)
                {
                    var tile = GameObjects.Tiles[idx];
                    var pen = new System.Drawing.Pen(tile.MarkColor, 1f);

                    for (var y = offsetY; y < snapshot.Height; y += tile.Height)
                    {
                        for (var x = offsetX; x < snapshot.Width; x += tile.Width)
                        {
                            for (var i = 0; i < tile.TilesetPositions.Count; i++)
                            {
                                var tilesetPosition = tile.TilesetPositions[i];

                                if (FindTile(snapshot, tileset, tile, tilesetPosition, x, y))
                                {
                                    using (var g = Graphics.FromImage(snapshot))
                                    {
                                        //g.DrawRectangle(pen, new Rectangle(x, y, tile.Width - 1, tile.Height - 1));
                                        var rectangle = new Rectangle(x, y, tile.Width - 1, tile.Height - 1);
                                        g.FillRectangle(new SolidBrush(pen.Color), rectangle);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DrawGrid(Bitmap snapshot, Point firstTilePosition)
        {
            using (var g = Graphics.FromImage(snapshot))
            {
                var pen = new System.Drawing.Pen(System.Drawing.Color.White, 1f);
                var offsetX = firstTilePosition.X % 16;
                var offsetY = firstTilePosition.Y % 16;

                for (var x = offsetX; x < snapshot.Width; x += 16)
                {
                    g.DrawLine(pen, x, 0, x, snapshot.Height - 1);
                }

                for (var y = offsetY; y < snapshot.Height; y += 16)
                {
                    g.DrawLine(pen, 0, y, snapshot.Width - 1, y);
                }

                var rectangle = new Rectangle(firstTilePosition.X, firstTilePosition.Y, 16, 16);
                g.FillRectangle(new SolidBrush(System.Drawing.Color.Blue), rectangle);
            }
        }

        private static Point? FindFirstTile(Bitmap snapshot)
        {
            var tileset = Tile.Tileset;

            for (var idx = 0; idx < GameObjects.Tiles.Count; idx++)
            {
                var tile = GameObjects.Tiles[idx];

                for (var y = snapshot.Height - tile.Height - 1; y > 0; y--)
                {
                    for (var x = 0; x < snapshot.Width; x++)
                    {
                        for (var i = 0; i < tile.TilesetPositions.Count; i++)
                        {
                            var tilesetPosition = tile.TilesetPositions[i];

                            if (FindTile(snapshot, tileset, tile, tilesetPosition, x, y))
                            {
                                return new Point(x, y);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static bool FindTile(Bitmap snapshot, Bitmap tileset, Tile tile, Point pos, int x, int y)
        {
            for (var x_tile = 1; x_tile < tile.Width - 2; x_tile++)
            {
                for (var y_tile = 1; y_tile < tile.Height - 2; y_tile++)
                {
                    if ((x + x_tile >= snapshot.Width) || (y + y_tile >= snapshot.Height) || !PixelsMatch(tileset.GetPixel(pos.X + x_tile, pos.Y + y_tile), snapshot.GetPixel(x + x_tile, y + y_tile)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool PixelsMatch(System.Drawing.Color spritePixel, System.Drawing.Color imagePixel)
        {
            return spritePixel.A < 255 || (spritePixel.R >= imagePixel.R - 40 && spritePixel.R <= imagePixel.R + 40 &&
                                           spritePixel.G >= imagePixel.G - 5 && spritePixel.G <= imagePixel.G + 5 &&
                                           spritePixel.B >= imagePixel.B - 5 && spritePixel.B <= imagePixel.B + 5);
        }
    }
}
