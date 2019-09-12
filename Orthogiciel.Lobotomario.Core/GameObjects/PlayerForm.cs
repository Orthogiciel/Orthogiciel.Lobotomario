using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core.GameObjects
{
    public class PlayerForm : GameObject
    {
        public static Bitmap Spritesheet
        {
            get { return Properties.Resources.Player; }
        }
    }
}
