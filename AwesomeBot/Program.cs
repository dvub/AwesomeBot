using System;
using System.Threading.Tasks;
using AwesomeBot;
using Serilog;

namespace AwesomeBot
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            await Startup.RunAsync(args);
        }
    }
}
