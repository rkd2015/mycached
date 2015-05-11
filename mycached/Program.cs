using System;
using System.Collections.Generic;
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

            cached.Run();
        }
    }
}
