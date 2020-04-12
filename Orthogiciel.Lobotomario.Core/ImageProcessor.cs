using Orthogiciel.Lobotomario.Core.GameObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public class ImageProcessor
    {
        private readonly GameObjectRepository gameObjectRepository;
        private readonly ObjectClassifier objectClassifier;
        private readonly Bitmap tileSpritesheet = Properties.Resources.Tileset;
        private readonly Bitmap playerSpritesheet = Properties.Resources.Player;
        private static readonly object snapshotLock = new object();

        public ImageProcessor(GameObjectRepository gameObjectRepository, ObjectClassifier objectClassifier)
        {
            this.gameObjectRepository = gameObjectRepository;
            this.objectClassifier = objectClassifier;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        // Recherche d'un pixel nous indiquant que Mario est peut-être à l'intérieur d'une région autour de point, puis recherche précise d'une sprite
        // de Mario dans les alentours. 
        // À améliorer : on pourrait garder en mémoire les zones déjà examinées pour ne pas repasser dessus.
        //-------------------------------------------------------------------------------------------------------------------------------------------------
        public Mario FindPlayer(Bitmap snapshot, GameState gameState)
        {
            var candidatePixels = new List<Point>();
            var previousPlayerBounds = gameState.CurrentState.FirstOrDefault(go => go.GetType() == typeof(Mario))?.Bounds;
            var playerColor = Color.FromArgb(255, 177, 52, 37);
            var x_start = previousPlayerBounds.HasValue && previousPlayerBounds.Value.X - 32 >= 0 ? previousPlayerBounds.Value.X - 32 : 0;
            var y_start = previousPlayerBounds.HasValue && previousPlayerBounds.Value.Y - 32 >= 0 ? previousPlayerBounds.Value.Y - 32 : 0;
            var x_end = previousPlayerBounds.HasValue && previousPlayerBounds.Value.X + previousPlayerBounds.Value.Width + 32 < snapshot.Width ? previousPlayerBounds.Value.X + previousPlayerBounds.Value.Width + 32 : snapshot.Width;
            var y_end = previousPlayerBounds.HasValue && previousPlayerBounds.Value.Y + previousPlayerBounds.Value.Height + 32 < snapshot.Height ? previousPlayerBounds.Value.Y + previousPlayerBounds.Value.Height + 32 : snapshot.Height;


            // Recherche les pixels rouges distincts dans l'image qui sont distants de plus de 16 pixels en x ou en y 
            // (on ne veut pas analyser inutilement le même groupe de pixels plusieurs fois).            
            for (var x = x_start; x < x_end && x < snapshot.Width; x++)
            {
                for (var y = y_end - 1; y > y_start; y--)
                {
                    if (PixelsMatch(playerColor, snapshot.GetPixel(x, y)))
                    {
                        if (!candidatePixels.Any(p => x - p.X < 16 || y - p.Y < 16))
                            candidatePixels.Add(new Point(x, y));
                    }
                }
            }

            // Tente de détecter une frame de Mario autour des pixels candidats trouvés
            foreach (Point point in candidatePixels)
            {
                for (int x = (point.X - 8 >= 0 ? point.X - 8 : 0); x < (point.X + 24 < snapshot.Width ? point.X + 24 : snapshot.Width - 1); x++)
                {
                    for (int y = (point.Y - 8 >= 0 ? point.Y - 8 : 0); y < (point.Y + 24 < snapshot.Height ? point.Y + 24 : snapshot.Height - 1); y++)
                    {
                        var classIndex = SearchAreaForMario(snapshot, x, y);

                        if (classIndex.HasValue)
                        {
                            var mario = objectClassifier.GetMario(classIndex.Value);
                            var bounds = new Rectangle(x, y, mario.Bounds.Width, mario.Bounds.Height);

                            return new Mario() { MarioForm = mario.MarioForm, Bounds = bounds, MarkColor = mario.MarkColor };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<Tile>> FindTiles(Bitmap snapshot, GameState gameState, int? playerDeltaX)
        {
            // TODO :
            // utiliser l'état précédent du GameState pour réduire les zones à analyser (par exemple : pas besoin de retraiter les blocs indestructibles)

            var firstTilePosition = (Point?)null;
            var tiles = new ConcurrentBag<Tile>();

            if (playerDeltaX.HasValue && playerDeltaX.Value == 0)
            {
                var firstTile = gameState.Tiles.FirstOrDefault();

                if (firstTile != null)
                    firstTilePosition = new Point(firstTile.Bounds.X, firstTile.Bounds.Y);
            }
            else
            {
                firstTilePosition = FindFirstTile(snapshot);
            }

            if (firstTilePosition != null)
            {
                var tasks = new List<Task>();

                var snapshotHeight = snapshot.Height;
                var snapshotWidth = snapshot.Width;
                var offsetX = firstTilePosition.Value.X % 16;
                var offsetY = firstTilePosition.Value.Y % 16;

                for (var y = offsetY; y + 15 < snapshotHeight; y = y + 16)
                {
                    for (var x = offsetX; x + 15 < snapshotWidth; x = x + 16)
                    {
                        var localX = x;
                        var localY = y;

                        tasks.Add(new Task(() => SearchAreaForTiles(snapshot, tiles, localX, localY)));
                    }
                }

                foreach (Task t in tasks)
                {
                    t.Start();
                }

                await Task.WhenAll(tasks.ToArray());
            }

            gameState.FirstTilePosition = firstTilePosition;

            return tiles.ToList();
        }

        private void SearchAreaForTiles(Bitmap snapshot, ConcurrentBag<Tile> tiles, int x, int y)
        {
            Bitmap imgSection;            

            lock (snapshotLock)
            {
                if (x + 16 >= snapshot.Width || y + 16 >= snapshot.Height)
                    return;

                imgSection = snapshot.Clone(new Rectangle(new Point(x, y), new Size(16, 16)), snapshot.PixelFormat);
            }

            var classIndex = (int)this.objectClassifier.ClassifyImage(imgSection);

            if (classIndex == ObjectClasses.BreakableBlock || classIndex == ObjectClasses.UnbreakableBlock)
            {
                var tileType = (TileTypes)classIndex;
                var tile = gameObjectRepository.Tiles.SingleOrDefault(t => t.TileType == tileType);
                tiles.Add(new Tile() { Bounds = new Rectangle(x, y, tile.Bounds.Width - 1, tile.Bounds.Height - 1), IsBreakable = tile.IsBreakable, IsCollidable = tile.IsCollidable, IsTuyo = tile.IsTuyo, Orientation = tile.Orientation, MarkColor = tile.MarkColor });
            }
        }

        private int? SearchAreaForMario(Bitmap snapshot, int x, int y)
        {
            Bitmap imgSection;

            lock (snapshotLock)
            {
                if (x + 16 >= snapshot.Width || y + 16 >= snapshot.Height)
                    return null;

                imgSection = snapshot.Clone(new Rectangle(new Point(x, y), new Size(16, 16)), snapshot.PixelFormat);
            }

            var classIndex = (int)this.objectClassifier.ClassifyImage(imgSection);

            if (classIndex == ObjectClasses.Mario || classIndex == ObjectClasses.SuperMario || classIndex == ObjectClasses.FieryMario || 
                classIndex == ObjectClasses.InvincibleMario)
                return classIndex;

            return null;
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

                                if (FindSprite(snapshot, tileSpritesheet, tile, tilesetPosition, x, y))
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
            for (var y = snapshot.Height - 16 - 1; y > 0; y--)
            {
                for (var x = 0; x < snapshot.Width - 16 - 1; x++)
                {
                    var imgSection = snapshot.Clone(new Rectangle(new Point(x, y), new Size(16, 16)), snapshot.PixelFormat);
                    var classIndex = (int)this.objectClassifier.ClassifyImage(imgSection);

                    if (classIndex == ObjectClasses.BreakableBlock || classIndex == ObjectClasses.UnbreakableBlock)
                    {
                        var tileType = (TileTypes)classIndex;
                        return new Point(x, y);
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
