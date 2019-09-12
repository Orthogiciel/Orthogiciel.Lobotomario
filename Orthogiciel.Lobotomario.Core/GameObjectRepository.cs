using Orthogiciel.Lobotomario.Core.GameObjects;
using System.Collections.Generic;
using System.Drawing;

namespace Orthogiciel.Lobotomario.Core
{
    public class GameObjectRepository
    {
        public List<Tile> Tiles { get; private set; }

        public List<Mario> Marios { get; private set; }

        public GameObjectRepository()
        {
            Marios = new List<Mario>()
            {
                new Mario()
                {
                    MarioForm = MarioForms.Mini,
                    Width = 16,
                    Height = 16,
                    SpritesheetPositions = new List<Point>()
                    {
                        new Point(80,35), new Point(97,34), new Point(114,34), new Point(131,34), new Point(148,34), new Point(165,34), new Point(182,34), new Point(199,34), new Point(216,34), new Point(233,34), new Point(250,34), new Point(267,34), new Point(284,34), new Point(301,34),
                        new Point(80,99), new Point(97,99), new Point(114,99), new Point(131,99), new Point(148,99), new Point(165,99), new Point(182,99), new Point(199,99), new Point(216,99), new Point(233,99), new Point(250,99), new Point(267,99), new Point(284,99), new Point(301,99)
                    },
                    MarkColor = Color.Pink
                },
                //new PlayerForm()
                //{
                //    PlayerForm = MarioForms.Super,
                //    Width = 16,
                //    Height = 32,
                //    SpritesheetPositions = new List<Point>()
                //    {
                //        new Point(80,1), new Point(97,1), new Point(114,1), new Point(131,1), new Point(148,1), new Point(165,1), new Point(182,1), new Point(199,1), new Point(216,1), new Point(233,1), new Point(250,1), new Point(267,1), new Point(284,1), new Point(301,1),
                //        new Point(80,66), new Point(97,66), new Point(114,66), new Point(131,66), new Point(148,66), new Point(165,66), new Point(182,66), new Point(199,66), new Point(216,66), new Point(233,66), new Point(250,66), new Point(267,66), new Point(284,66), new Point(301,66)
                //    },
                //    MarkColor = Color.LightSkyBlue
                //}
            };

            Tiles = new List<Tile>()
            {                
                new Tile()
                {
                    TileType = TileTypes.UnbreakableBlock,
                    Width = 16,
                    Height = 16,
                    SpritesheetPositions = new List<Point>()
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
                    TileType = TileTypes.BreakableBlock,
                    Width = 16,
                    Height = 16,
                    SpritesheetPositions = new List<Point>()
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
