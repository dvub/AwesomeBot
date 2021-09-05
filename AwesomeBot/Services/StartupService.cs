using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using System.Diagnostics;
using Discord.WebSocket;
using System.IO;
using Core;
using System.Collections.Generic;
using Serilog;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;
using Destructurama.Attributed;
using Serilog.Formatting.Json;
using System.Threading;
using Destructurama;
using Serilog.Enrichers;
namespace AwesomeBot.Services
{
    /// <summary>
    /// Service to start bot.
    /// </summary>
    public class StartupService
    {
        //dependency injection
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        private readonly CommandService _command;
        private readonly AppSettingsRoot _config;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, AppSettingsRoot config)
        {
            _provider = provider;
            _discord = discord;
            _command = commands;
            _config = config;

        }
        /// <summary>
        /// Starts the bot.
        /// </summary>
        public async Task StartAsync()
        {

            string token = _config.TokenString;
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("please provide discord token");
                return;
            }

            await _discord.LoginAsync(Discord.TokenType.Bot, token); //login
            await _discord.StartAsync(); //start bot

            ConfigureLogging();
            StartLavaLink(); //run LavaLink server
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            
            
            await _discord.SetGameAsync("I'm a bot", null, Discord.ActivityType.Playing); //set status
            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "ThreadId", Thread.CurrentThread.ManagedThreadId));
            }
        }
        public void ConfigureLogging()
        {
            string OutputTemplate = "{NewLine}{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:j}{NewLine}{Exception}{Properties:j}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: OutputTemplate)
                .WriteTo.File(path: $"{AppContext.BaseDirectory}log.txt", outputTemplate: OutputTemplate)
                .CreateLogger();
        }
        /// <summary>
        /// Runs LavaLink server using a batch file
        /// </summary>
        public void StartLavaLink()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(AppContext.BaseDirectory, "runServer.bat"); //run batch file to start LavaLink
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}