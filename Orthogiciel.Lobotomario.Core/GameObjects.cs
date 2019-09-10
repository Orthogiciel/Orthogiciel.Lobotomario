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
                new Tile()
                {
                    Id = TileId.UnbreakableBlock,
                    Width = 16,
                    Height = 16,
                    TilesetPositions = new List<Point>()
                    {
                        new Point(0,0), new Point(0,32), new Point(0,64), new Point(0,96),
                        new Point(0,16), new Point(0,48), new Point(0,80), new Point(0,112),
                        new Point(48,0), new Point(48,32), new Point(48,64), new Point(48,96),
                        new Point(432,-1), new Point(432,31), new Point(432,63), new Point(432,95)
                    },
                    IsCollidable = true,
                    MarkColor = Color.Blue
                },
                new Tile()
                {
                    Id = TileId.BreakableBlock,
                    Width = 16,
                    Height = 16,
                    TilesetPositions = new List<Point>()
                    {
                        new Point(16,0), new Point(16,32), new Point(16,64), new Point(16,96),
                        new Point(32,0), new Point(32,32), new Point(32,64), new Point(32,96)
                    },
                    IsCollidable = true,
                    IsBreakable = true,
                    MarkColor = Color.Yellow
                }                
            };
        }
    }
}
