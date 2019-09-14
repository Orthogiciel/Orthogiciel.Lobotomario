using Orthogiciel.Lobotomario.Core.GameObjects;
using System.Collections.Generic;
using System.Drawing;

namespace Orthogiciel.Lobotomario.Core
{
    public class ImageProcessor
    {
        private readonly GameObjectRepository gameObjectRepository;

        public ImageProcessor(GameObjectRepository gameObjectRepository)
        {
            this.gameObjectRepository = gameObjectRepository;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        // Recherche d'un pixel nous indiquant que Mario est peut-être à l'intérieur d'une région autour de point, puis recherche précise d'une sprite
        // de Mario dans les alentours. 
        // À améliorer : on pourrait garder en mémoire les zones déjà examinées pour ne pas repasser dessus.
        //-------------------------------------------------------------------------------------------------------------------------------------------------
        public Mario FindPlayer(Bitmap snapshot)
        {
            var spritesheet = Mario.Spritesheet;
            var playerColor = Color.FromArgb(255, 177, 52, 37);

            for (var x = 0; x < snapshot.Width; x++)
            {
                for (var y = snapshot.Height - 1; y > 0; y--)
                {
                    if (PixelsMatch(playerColor, snapshot.GetPixel(x, y)))
                    {
                        // Vérifie si on peut trouver une occurence d'une des sprites de Mario
                        foreach (Mario mario in gameObjectRepository.Marios)
                        {
                            var bounds = FindGameObjectInZone(snapshot, new Rectangle(x - 8, y - 31, 23, 62), mario, spritesheet);

                            if (bounds.HasValue)
                            {
                                return new Mario() { MarioForm = mario.MarioForm, Bounds = bounds.Value };
                            }
                        }

                        y -= 31;
                    }
                }
            }

            return null;
        }

        public List<Tile> FindTiles(Bitmap snapshot)
        {
            var tiles = new List<Tile>();
            var firstTilePosition = FindFirstTile(snapshot);

            if (firstTilePosition != null)
            {
                var tileset = Tile.Tileset;
                var offsetX = firstTilePosition.Value.X % 16;
                var offsetY = firstTilePosition.Value.Y % 16;

                for (var idx = 0; idx < gameObjectRepository.Tiles.Count; idx++)
                {
                    var tile = gameObjectRepository.Tiles[idx];

                    for (var y = offsetY; y < snapshot.Height; y += tile.Bounds.Height)
                    {
                        for (var x = offsetX; x < snapshot.Width; x += tile.Bounds.Width)
                        {
                            for (var i = 0; i < tile.SpritesheetPositions.Count; i++)
                            {
                                var tilesetPosition = tile.SpritesheetPositions[i];

                                if (FindSprite(snapshot, tileset, tile, tilesetPosition, x, y))
                                {
                                    tiles.Add(new Tile() { Bounds = new Rectangle(x, y, tile.Bounds.Width - 1, tile.Bounds.Height - 1), IsBreakable = tile.IsBreakable, IsCollidable = tile.IsCollidable, IsTuyo = tile.IsTuyo, Orientation = tile.Orientation });
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return tiles;
        }

        public void MarkPlayer(Bitmap snapshot)
        {
            //var spritesheet = PlayerForm.Spritesheet;
            var playerColor = System.Drawing.Color.FromArgb(255, 177, 52, 37);
            var pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1f);

            for (var x = 0; x < snapshot.Width; x++)
            {
                for (var y = snapshot.Height - 1; y > 0; y--)
                {
                    if (PixelsMatch(playerColor, snapshot.GetPixel(x, y)))
                    {
                        using (var g = Graphics.FromImage(snapshot))
                        {
                            g.DrawRectangle(pen, new Rectangle(x - 4, y - 16, 16, 16));
                            return;
                        }
                    }
                }
            }
        }

        public void MarkTiles(Bitmap snapshot, bool drawGrid)
        {
            var firstTilePosition = FindFirstTile(snapshot);

            if (firstTilePosition != null)
            {
                if (drawGrid)
                    DrawGrid(snapshot, firstTilePosition.Value);

                var tileset = Tile.Tileset;                
                var offsetX = firstTilePosition.Value.X % 16;
                var offsetY = firstTilePosition.Value.Y % 16;                

                for (var idx = 0; idx < gameObjectRepository.Tiles.Count; idx++)
                {
                    var tile = gameObjectRepository.Tiles[idx];
                    var pen = new System.Drawing.Pen(tile.MarkColor, 1f);

                    for (var y = offsetY; y < snapshot.Height; y += tile.Bounds.Height)
                    {
                        for (var x = offsetX; x < snapshot.Width; x += tile.Bounds.Width)
                        {
                            for (var i = 0; i < tile.SpritesheetPositions.Count; i++)
                            {
                                var tilesetPosition = tile.SpritesheetPositions[i];

                                if (FindSprite(snapshot, tileset, tile, tilesetPosition, x, y))
                                {
                                    using (var g = Graphics.FromImage(snapshot))
                                    {
                                        //g.DrawRectangle(pen, new Rectangle(x, y, tile.Width - 1, tile.Height - 1));
                                        var rectangle = new Rectangle(x, y, tile.Bounds.Width - 1, tile.Bounds.Height - 1);
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

        public void DrawGrid(Bitmap snapshot, Point firstTilePosition)
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

        private Rectangle? FindGameObjectInZone(Bitmap snapshot, Rectangle bounds, GameObject gameObject, Bitmap spritesheet)
        {
            foreach (Point spritePos in gameObject.SpritesheetPositions)
            {
                for (var x = bounds.X; x < bounds.X + bounds.Width; x++)
                {
                    for (var y = bounds.Y; y < bounds.Y + bounds.Height; y++)
                    {
                        var isMatch = true;

                        for (var x_sprite = 1; x_sprite < gameObject.Bounds.Width - 1; x_sprite++)
                        {
                            for (var y_sprite = 1; y_sprite < gameObject.Bounds.Height - 1; y_sprite++)
                            {
                                if (x + x_sprite < 0 || x + x_sprite >= snapshot.Width || y + y_sprite < 0 || y + y_sprite >= snapshot.Height)
                                {
                                    isMatch = false;
                                    break;
                                }

                                if (!PixelsMatch(spritesheet.GetPixel(spritePos.X + x_sprite, spritePos.Y + y_sprite), snapshot.GetPixel(x + x_sprite, y + y_sprite)))
                                {
                                    isMatch = false;
                                    break;
                                }
                            }

                            if (!isMatch)
                                break;
                        }

                        if (isMatch)
                            return new Rectangle(x, y, gameObject.Bounds.Width, gameObject.Bounds.Height);
                    }
                }
            }

            return null;
        }

        private Point? FindFirstTile(Bitmap snapshot)
        {
            var tileset = Tile.Tileset;

            for (var idx = 0; idx < gameObjectRepository.Tiles.Count; idx++)
            {
                var tile = gameObjectRepository.Tiles[idx];

                for (var y = snapshot.Height - tile.Bounds.Height - 1; y > 0; y--)
                {
                    for (var x = 0; x < snapshot.Width; x++)
                    {
                        for (var i = 0; i < tile.SpritesheetPositions.Count; i++)
                        {
                            var tilesetPosition = tile.SpritesheetPositions[i];

                            if (FindSprite(snapshot, tileset, tile, tilesetPosition, x, y))
                            {
                                return new Point(x, y);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private bool FindSprite(Bitmap snapshot, Bitmap tileset, GameObject gameObject, Point pos, int x, int y)
        {
            for (var x_object = 1; x_object < gameObject.Bounds.Width - 2; x_object++)
            {
                for (var y_object = 1; y_object < gameObject.Bounds.Height - 2; y_object++)
                {
                    if ((x + x_object >= snapshot.Width) || (y + y_object >= snapshot.Height) || !PixelsMatch(tileset.GetPixel(pos.X + x_object, pos.Y + y_object), snapshot.GetPixel(x + x_object, y + y_object)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool PixelsMatch(System.Drawing.Color spritePixel, System.Drawing.Color imagePixel)
        {
            return (spritePixel.A < 255) || (spritePixel.R >= imagePixel.R - 40 && spritePixel.R <= imagePixel.R + 40 &&
                                             spritePixel.G >= imagePixel.G - 5 && spritePixel.G <= imagePixel.G + 5 &&
                                             spritePixel.B >= imagePixel.B - 5 && spritePixel.B <= imagePixel.B + 5);
        }
    }
}
