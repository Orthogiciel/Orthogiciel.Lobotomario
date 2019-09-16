using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Orthogiciel.Lobotomario.Core
{
    public class Engine
    {
        private readonly BackgroundWorker backgroundWorker;

        private const string processName = "Mesen";

        private readonly Process emulatorProcess;

        private readonly Screen screen;

        private readonly GameObjectRepository gameObjectRepository;

        private readonly ImageProcessor imageProcessor;

        private readonly Input input;

        private readonly GameState gameState;

        private bool isRunning;

        public event EventHandler<GameState> Updated;

        public Engine()
        {
            try
            {
                emulatorProcess = Process.GetProcessesByName(processName).FirstOrDefault();

                screen = new Screen(emulatorProcess);
                gameObjectRepository = new GameObjectRepository();
                imageProcessor = new ImageProcessor(gameObjectRepository);
                input = new Input();
                gameState = new GameState();
                
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += DoWork;
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem initializing engine : {ex.Message}\r\n{ex.InnerException?.Message}");
            }
        }

        public void Start()
        {
            try
            {
                backgroundWorker.RunWorkerAsync();
                isRunning = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem starting engine : {ex.Message}\r\n{ex.InnerException?.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                isRunning = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem stopping engine : {ex.Message}\r\n{ex.InnerException?.Message}");
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while (isRunning)
            {
                try
                {
                    var snapshot = screen.TakeSnapshot();

                    // Ici au lieu de dessinger par dessus le screenshot, il faut updater le GameState avec la position de chaque élément
                    // trouvé.

                    var newGameState = new List<GameObject>();
                    var player = imageProcessor.FindPlayer(snapshot, gameState);
                    var playerDeltaX = (player == null || gameState.Player == null) ? null : (int?)(player.Bounds.X - gameState.Player.Bounds.X);
                    var tiles = (playerDeltaX.HasValue && playerDeltaX.Value == 0) ? gameState.Tiles : imageProcessor.FindTiles(snapshot, gameState, playerDeltaX);

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
                catch(Exception ex)
                {
                    throw new Exception($"Engine processing error : {ex.Message}\r\n{ex.InnerException?.Message}");
                }
            }
        }
    }
}
