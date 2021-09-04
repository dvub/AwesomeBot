using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Common;
using Common.Extensions;
using Victoria;
using Common.Types;
using Infrastructure;

namespace AwesomeBot.Services
{
    //TODO: LOGGING
    //  (MAKE IT GOOD)

    /// <summary>
    /// Service to handle commands and mutes
    /// </summary>
    public class CommandHandler
    {
        //dependency injection
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        public static CommandService _command;
        private readonly LavaNode _lavaNode;
        public static List<Mute> Mutes = new List<Mute>();
        private readonly ServerService _servers;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, LavaNode lavaNode, ServerService servers)
        {
            _provider = provider;
            _discord = discord;
            _command = commands;
            _lavaNode = lavaNode;
            _servers = servers;
            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();
            _discord.Ready += OnReady;
            _discord.Ready += onReadyAsync;
            _discord.MessageReceived += _discord_MessageReceived;
            _lavaNode.OnTrackEnded += _lavaNode_OnTrackEnded;
            _discord.UserJoined += _discord_UserJoined;
            _command.CommandExecuted += _command_CommandExecuted;

        }

        private async Task _command_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            var channel = arg2.Channel;
            if (arg3.IsSuccess) return;
            //check for errors, send error msg
            switch (arg3.Error)
            {
                case CommandError.UnknownCommand:
                    await channel.SendMessageAsync("I don't recognize that command!");
                    return;
                case CommandError.BadArgCount:
                    await channel.SendMessageAsync("Please provide correct arguments for command!");
                    return;
                case CommandError.UnmetPrecondition:
                    await channel.SendMessageAsync("You don't have permission to use that command!");
                    return;
                case CommandError.ParseFailed:
                    await channel.SendMessageAsync("Please provide correct arguments for command and ensure spelling is correct!");
                    return;
                case CommandError.ObjectNotFound:
                    await channel.SendMessageAsync("Please provide correct arguments for command and ensure spelling is correct!");
                    return;
                default:
                    await channel.SendMessageAsync("An error occurred. Make sure your spelling is correct.");
                    return;

            }
        }

        private async Task _discord_UserJoined(SocketGuildUser arg)
        {

            ulong id = arg.Guild.Id;
            var server = _servers.servers.Find(x => x.Id == id);
            if (server == null)
            {
                await _servers.ModifyGuildGreeting(id, "");
                await _servers.ModifyGuildGreetingType(id, "Disabled");
                server = _servers.servers.Find(x => x.Id == id);

            }
            //do greeting based on server settings
            switch (server.GreetingType)
            {
                case GreetingType.Channel:
                    await (arg.Guild.Channels.ToList().Find(x => x.Id == server.GreetingChannelId) as SocketTextChannel).SendMessageAsync(server.Greeting);
                    return;
                case GreetingType.DM:
                    await arg.SendMessageAsync(server.Greeting);
                    return;
                case GreetingType.Disabled:
                    return;
            }

        }

        private async Task _lavaNode_OnTrackEnded(Victoria.EventArgs.TrackEndedEventArgs arg)
        {
            Console.WriteLine("Track ended:" + arg.Reason);
            //if looping is set to true, start playing from beginning
            if (Modules.Media.isLooping)
            {
                if (arg.Reason == Victoria.Enums.TrackEndReason.Stopped)
                {
                    Modules.Media.isLooping = false;
                    return;
                }
                await arg.Player.PlayAsync(arg.Track);
            }

        }
        private async Task MuteHandler()
        {
            //add and remove mutes for admin
            List<Mute> Remove = new List<Mute>();
            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;
                var guild = _discord.GetGuild(mute.Guild.Id);
                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (guild.GetRole(mute.Role.Id).Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }
                await mute.User.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }
            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }

        private async Task _discord_MessageReceived(SocketMessage arg)
        {
            //make sure msgs arent sent by bots or the server, get prefix and run commands
            var msg = arg as SocketUserMessage;

            if (msg != null && !msg.Author.IsBot)
            {
                ulong serverId = new ulong();
                if (!(arg.Channel is IPrivateChannel))
                {
                    serverId = (msg.Channel as SocketGuildChannel).Guild.Id;
                    var server = _servers.servers.Find(x => x.Id == serverId);
                    if (server == null)
                    {
                        await _servers.ModifyGuildPrefix(serverId, "!");
                        server = _servers.servers.Find(x => x.Id == serverId);
                    }
                    if (server.CommandChannelId != msg.Channel.Id && server.CommandChannelId != null)
                    {
                        var channel = msg.Channel as SocketGuildChannel;

                        await msg.Channel.SendMessageAsync($"Please use commands in {channel.Guild.Channels.ToList().Find(x => x.Id == server.CommandChannelId).Name ?? channel.Guild.DefaultChannel.Name}");
                        return;
                    }
                    string prefix = "";
                    prefix = server.Prefix;
                    var context = new SocketCommandContext(_discord, msg);
                    int pos = 0;
                    if (msg.HasStringPrefix(prefix, ref pos) || msg.HasMentionPrefix(_discord.CurrentUser, ref pos))
                    {

                        var result = await _command.ExecuteAsync(context, pos, _provider);
                        if (!result.IsSuccess)
                        {
                            var reason = result.Error;
                            string errorMessage = $"The following error occurred: \n {reason}";
                            Console.WriteLine(errorMessage);
                        }

                    }
                }
                else
                {
                    await arg.Author.SendMessageAsync("DMs isn't implemented yet, sorry");
                }
            }
            else
            {
                Console.WriteLine("Message sent by server or bot");
            }
        }
        //start lavanode basically
        private async Task onReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private Task OnReady()
        {

            Console.WriteLine($"Connected as {_discord.CurrentUser.Username}#{_discord.CurrentUser.Discriminator}");
            return Task.CompletedTask;
        }
    }
}