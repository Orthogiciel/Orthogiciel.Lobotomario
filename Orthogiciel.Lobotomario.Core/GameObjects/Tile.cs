using System.Drawing;

namespace Orthogiciel.Lobotomario.Core.GameObjects
{
    public class Tile : GameObject
    {
        public TileTypes TileType { get; set; }

        public static Bitmap Tileset
        {
            get { return Properties.Resources.Tileset; }
        }

        public bool IsCollidable { get; set; }

        public bool IsBreakable { get; set; }

        public bool IsTuyo { get; set; }        

        // Pour les Tuyos
        public Orientation Orientation { get; set; }
    }
}
