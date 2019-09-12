using System.Collections.Generic;
using System.Drawing;

namespace Orthogiciel.Lobotomario.Core
{
    public abstract class GameObject
    {
        public int Id { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public List<Point> SpritesheetPositions { get; set; }

        public Color MarkColor { get; set; }
    }
}
