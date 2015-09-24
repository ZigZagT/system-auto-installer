using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SystemAutoInstall
{
    public class ThreadControl
    {
        public static void Task(Action task, Action complete)
        {
            Action run = () =>
            {
                var t = new Thread(new ThreadStart(task));
                t.Start();
                t.Join();
                complete();
            };
            var oThread = new Thread(new ThreadStart(run));
            oThread.Start();
        }
    }
}
