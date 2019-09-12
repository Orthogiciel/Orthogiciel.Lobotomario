using System;
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
                imageProcessor = new ImageProcessor();
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

                    imageProcessor.MarkPlayer(snapshot); // On ne veut plus dessiner par dessus les screenshots.
                    imageProcessor.MarkTiles(snapshot, false); // On ne veut plus dessiner par dessus les screenshots.

                    // Update game state here.

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
