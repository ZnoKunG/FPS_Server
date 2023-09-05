using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FPS_Server
{
    internal class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "FPS Server";
            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(5, 2546);
        }
        private static void MainThread()
        {
            Console.WriteLine($"Main Thread started. Running at {Constants.TICKS_PER_SEC} ticks per second");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();
                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                }
            }
        }
    }

}
