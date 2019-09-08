using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public class Tile
    {
        public static Bitmap Tileset
        {
            get { return Properties.Resources.Tileset; }
        }

        public int Id { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public List<Point> TilesetPositions { get; set; }

        public bool IsCollidable { get; set; }

        public bool IsBreakable { get; set; }

        public bool IsTuyo { get; set; }

        public Color MarkColor { get; set; }

        // Pour les Tuyos
        public Orientation Orientation { get; set; }
    }
}
