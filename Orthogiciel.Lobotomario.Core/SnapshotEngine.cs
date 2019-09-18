using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace Orthogiciel.Lobotomario.Core
{
    public class SnapshotEngine : Engine
    {
        public event EventHandler<Image> Updated;

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            while (isRunning)
            {
                try
                {
                    Updated?.Invoke(this, screen.TakeSnapshot());
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
