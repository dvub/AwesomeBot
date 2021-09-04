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
            string path = $"{AppContext.BaseDirectory}/LavaLink/Lavalink.jar";

            startLavaLink(); //run LavaLink server
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($@"{AppContext.BaseDirectory}log.txt")
                .CreateLogger();
            await _discord.SetGameAsync("I'm a bot", null, Discord.ActivityType.Playing); //set status
            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        
        /// <summary>
        /// Runs LavaLink server using a batch file
        /// </summary>
        public void startLavaLink()
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