using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Orthogiciel.Lobotomario.Core
{
    public class GameStateEngine : Engine
    {
        public event EventHandler<GameState> Updated;

        protected async override void DoWork(object sender, DoWorkEventArgs e)
        {
            while (isRunning)
            {
                try
                {
                    var snapshot = screen.TakeSnapshot();

                    // Ici au lieu de dessiner par dessus le screenshot, il faut updater le GameState avec la position de chaque élément
                    // trouvé.

                    var newGameState = new List<GameObject>();
                    var player = imageProcessor.FindPlayer(snapshot, gameState);
                    var playerDeltaX = (player == null || gameState.Player == null) ? null : (int?)(player.Bounds.X - gameState.Player.Bounds.X);
                    var tiles = (playerDeltaX.HasValue && playerDeltaX.Value == 0) ? gameState.Tiles : await imageProcessor.FindTiles(snapshot, gameState, playerDeltaX);

                    if (player != null)
                    {
                        newGameState.Add(player);
                    }

                    newGameState.AddRange(tiles);

                    // Update game state here.

                    gameState.PreviousState = gameState.CurrentState;
                    gameState.CurrentState = newGameState;

                    Updated?.Invoke(this, gameState);

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Engine processing error : {ex.Message}\r\n{ex.InnerException?.Message}");
                }
            }
        }
    }
}
