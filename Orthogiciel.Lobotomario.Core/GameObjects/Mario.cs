using System.Drawing;

namespace Orthogiciel.Lobotomario.Core.GameObjects
{
    public class Mario : GameObject
    {
        public MarioForms PlayerForm { get; set; }
        
        public static Bitmap Spritesheet
        {
            get { return Properties.Resources.Player; }
        }
    }
}
