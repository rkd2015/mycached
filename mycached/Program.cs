using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached
{
    class Program
    {
        static void Main(string[] args)
        {
            MyCached cached = new MyCached();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cached.Stop();
            };

            Trace.TraceInformation("Starting mycached service...");
            cached.Run();
            Trace.TraceInformation("Exiting mycached service...");
        }
    }
}
