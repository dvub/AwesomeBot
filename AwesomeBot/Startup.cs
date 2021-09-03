using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using AwesomeBot.Services;
using Victoria;
using Interactivity ;
using Infrastructure;
using Core;
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
                LogLevel = Discord.LogSeverity.Info,
                MessageCacheSize = 1000
            }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    
                    LogLevel = Discord.LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false,
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddLavaNode(x =>
                {
                    x.ReconnectAttempts = 3;
                })
                .AddSingleton<InteractivityService>()
                .AddSingleton<ServerService>()
                .AddDbContext<Context>()
                .AddSingleton(AppSettingsRoot.IsCreated
                    ? AppSettingsRoot.Load()
                    : AppSettingsRoot.Create());
                
            
        }
    }
}
