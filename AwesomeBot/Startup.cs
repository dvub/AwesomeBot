using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using AwesomeBot.Services;
using Victoria;
using Discord.Addons.Interactive;
using Infrastructure;
using AwesomeBot.Core;
using Infrastructure.Migrations;
namespace AwesomeBot
{
    public class Startup
    {
        public Startup(string[] args) 
        {

        }
        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);

            await startup.RunAsync();
        }
        public async Task RunAsync()
        {


            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<CommandHandler>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Debug,
                MessageCacheSize = 1000
            }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = Discord.LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false
                }))

                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddLavaNode(x =>
                {
                    x.SelfDeaf = true;

                })
                .AddSingleton<InteractiveService>()
                .AddDbContext<Context>()
                .AddSingleton<AppSettingsRoot>()
                .AddSingleton<Servers>();
                
            
        }
    }
}
