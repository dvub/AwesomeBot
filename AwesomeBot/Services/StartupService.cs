using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Diagnostics;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using System.IO;

namespace AwesomeBot.Services
{
    public class StartupService
    {
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        private readonly CommandService _command;
        private readonly IConfigurationRoot _config;


        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, IConfigurationRoot config )
        {
            _provider = provider;
            _discord = discord;
            _command = commands;
            _config = config;

        }

        public async Task StartAsync()
        {
            string token = _config["token"];
            if(string.IsNullOrEmpty(token))
            {
                Console.WriteLine("please provide discord token");
                return;
            }
           
            await _discord.LoginAsync(Discord.TokenType.Bot, token);
            await _discord.StartAsync();
            startLavaLink();
            await _discord.SetGameAsync("I'm a bot", null, Discord.ActivityType.Playing);

            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        public void startLavaLink()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\Kaya\Documents\Programs\AwesomeBot\AwesomeBot\runServer.bat";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.UseShellExecute = true ;
            Process.Start(psi);
        }
    }
}
