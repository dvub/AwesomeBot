using System;
using System.Threading.Tasks;
using AwesomeBot;

namespace AwesomeBot
{
    class Program
    {
        public static async Task Main(string[] args)
            => await Startup.RunAsync(args);
    }
}
