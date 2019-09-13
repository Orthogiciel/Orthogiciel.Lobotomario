using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public class GameState
    {
        public List<GameObject> CurrentState { get; set; }

        public List<GameObject> PreviousState { get; set; }

        public GameState()
        {
            this.CurrentState = new List<GameObject>();
            this.PreviousState = new List<GameObject>();
        }
    }
}
