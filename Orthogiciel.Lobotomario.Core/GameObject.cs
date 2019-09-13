using System.Collections.Generic;
using System.Drawing;

namespace Orthogiciel.Lobotomario.Core
{
    public abstract class GameObject
    {
        public Rectangle Bounds { get; set; }

        public List<Point> SpritesheetPositions { get; set; }

        public Color MarkColor { get; set; }
    }
}
