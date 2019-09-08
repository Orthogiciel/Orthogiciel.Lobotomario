using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public static class GameObjects
    {
        public static List<Tile> Tiles { get; private set; }

        static GameObjects()
        {
            Tiles = new List<Tile>()
            {
                { new Tile() { Id = TileId.Block, Width = 16, Height = 16, TilesetPositions = new List<Point>() { new Point(0,0), new Point(0,32), new Point(0,64), new Point(0,96) }, IsCollidable = true, MarkColor = Color.Blue } }
            };
        }
    }
}
