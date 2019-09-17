using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public class SnapshotEngine : Engine
    {
        public event EventHandler<Image> Updated;

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            
        }
    }
}
