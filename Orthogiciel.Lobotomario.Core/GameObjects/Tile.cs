using System.Drawing;

namespace Orthogiciel.Lobotomario.Core.GameObjects
{
    public class Tile : GameObject
    {
        public TileTypes TileType { get; set; }

        public bool IsCollidable { get; set; }

        public bool IsBreakable { get; set; }

        public bool IsTuyo { get; set; }        

        // Pour les Tuyos
        public Orientation Orientation { get; set; }
    }
}
