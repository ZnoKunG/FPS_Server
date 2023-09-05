using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FPS_Server
{
    class GameLogic
    {
        public static void Update()
        {
            ThreadManager.UpdateMainThread();
        }
    }
}
