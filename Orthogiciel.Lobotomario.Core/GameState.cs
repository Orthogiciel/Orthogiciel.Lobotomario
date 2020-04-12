using Orthogiciel.Lobotomario.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthogiciel.Lobotomario.Core
{
    public class GameState
    {
        public Point? FirstTilePosition { get; set; }

        public List<GameObject> CurrentState { get; set; }

        public List<GameObject> PreviousState { get; set; }

        public Mario Player
        {
            get { return (Mario)CurrentState.FirstOrDefault(o => o.GetType() == typeof(Mario)); }
        }

        public List<Tile> Tiles
        {
            get { return CurrentState.Where(o => o.GetType() == typeof(Tile)).OfType<Tile>().ToList(); }
        }

        public GameState()
        {
            this.CurrentState = new List<GameObject>();
            this.PreviousState = new List<GameObject>();
        }
    }
}
