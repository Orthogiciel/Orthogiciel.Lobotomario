using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Orthogiciel.Lobotomario.Core
{
    public abstract class Engine
    {
        protected readonly BackgroundWorker backgroundWorker;

        protected const string processName = "Mesen";

        protected readonly Process emulatorProcess;

        protected readonly Screen screen;

        protected readonly GameObjectRepository gameObjectRepository;

        protected readonly ImageProcessor imageProcessor;

        protected readonly ObjectClassifier objectClassifier;

        protected readonly Input input;

        protected readonly GameState gameState;

        protected bool isRunning;

        public Engine()
        {
            try
            {
                emulatorProcess = Process.GetProcessesByName(processName).FirstOrDefault();

                screen = new Screen(emulatorProcess);
                gameObjectRepository = new GameObjectRepository();

                objectClassifier = new ObjectClassifier(gameObjectRepository);
                imageProcessor = new ImageProcessor(gameObjectRepository, objectClassifier);
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

        protected abstract void DoWork(object sender, DoWorkEventArgs e);
    }
}
