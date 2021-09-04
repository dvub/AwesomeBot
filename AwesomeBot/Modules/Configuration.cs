using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading;
using AwesomeBot.Services;
using System.Reflection;
using System.Text.RegularExpressions;
using Infrastructure;
using Serilog;

namespace AwesomeBot.Modules
{
    [Summary("Configure your server! Set a greeting, command prefix, and much more!")]
    public class Configuration : ModuleBase<SocketCommandContext>
    {
        private readonly ServerService _servers;
        public Configuration(ServerService servers)
        {
            _servers = servers;
        }
        [Command("prefix")]
        [Summary("Set prefix for commands!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix([Remainder] string prefix = null)
        {
            if (prefix == null)
            {
                var GuildPrefix = _servers.servers.Find(x => x.Id == Context.Guild.Id).Prefix;
                await ReplyAsync($"The current prefix for this bot is `{GuildPrefix}`");
                return;
            }
            if (prefix.Length > 8)
            {
                await ReplyAsync("Length of the new prefix is too long!");
                return;
            }
            try
            {
                await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
                await ReplyAsync($"The prefix has been changed to `{prefix}`");
            }
            catch (Exception e)
            {
                Log.Error($"An error occurred:\n{e}");
            }

        }
        [Command("greetingtype")]
        [Summary("Sets the type of greeting users receive when joining. Use DM, Channel, or Disabled.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGreetingTypeAsync([Remainder] string type = null)
        {
            if (type == null)
            {
                var _type = _servers.servers.Find(x => x.Id == Context.Guild.Id).GreetingType;
                if (_type == GreetingType.Disabled)
                {
                    await ReplyAsync($"Greeting users is disabled");
                    return;
                }
                await ReplyAsync($"Set to greet users in {_type}");

            }
            else
            {
                try
                {
                    await _servers.ModifyGuildGreetingType(Context.Guild.Id, type);
                    await ReplyAsync($"Updated greeting type to {type}");
                }
                catch (Exception e)
                {
                    await ReplyAsync("Something went wrong, try again");
                    Log.Error($"An error occurred:\n{e}");
                }

            }
        }
        [Command("greeting")]
        [Summary("Sets the message when a user joins a server.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGreetingAsync([Remainder] string message = null)
        {
            if (message == null)
            {
                var _message = _servers.servers.Find(x => x.Id == Context.Guild.Id).Greeting ?? "No greeting set";
                if (_servers.servers.Find(x => x.Id == Context.Guild.Id).GreetingType == GreetingType.Disabled)
                {
                    await ReplyAsync($"Greeting users is disabled");
                    return;
                }
                await ReplyAsync($"Greeting is {_message}");
            }
            else
            {
                await _servers.ModifyGuildGreeting(Context.Guild.Id, message);
                await ReplyAsync($"Modified greeting to be {message}");
            }
        }
        [Command("Greetingchannel")]
        [Summary("Sets the channel ID where users are greeted if greetingtype is set to channel. Right click the channel name and Copy ID to get the ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGreetingChannel([Remainder] string id = null)
        {
            if (id == null)
            {
                try
                {
                    var channel = Context.Guild.Channels.ToList().Find(x => x.Id == ulong.Parse(id)).Name;
                    await ReplyAsync($"Channel for greeting is set to {channel ?? Context.Guild.DefaultChannel.Name}");
                }
                catch (Exception e)
                {
                    await ReplyAsync("Something went wrong");
                    Log.Error($"An error occurred:\n{e}");
                }
            }
            else
            {
                try
                {
                    await _servers.ModifyGreetingChannelId(Context.Guild.Id, ulong.Parse(id));
                }
                catch (Exception e)
                {
                    await ReplyAsync("Please provide a valid channel id");
                    Log.Error($"An error occurred:\n{e}");
                }
            }
        }
        [Command("channel")]
        [Summary("Sets channel ID where commands can be used. Right click the channel name and Copy ID to get the ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetCommandChannelId([Remainder] string id = null)
        {
            if (id == null)
            {
                var channelid = _servers.servers.Find(x => x.Id == Context.Guild.Id).CommandChannelId ?? Context.Guild.DefaultChannel.Id;
                var channel = Context.Guild.Channels.ToList().Find(x => x.Id == channelid).Name;

                await ReplyAsync($"Channel for commands is set to {channel ?? Context.Guild.DefaultChannel.Name}");
            }
            else
            {
                try
                {
                    await _servers.ModifyCommandsChannelId(Context.Guild.Id, ulong.Parse(id));
                    await ReplyAsync($"Commands can now only be sent in {Context.Guild.Channels.ToList().Find(x => x.Id == _servers.servers.Find(x => x.Id == Context.Guild.Id).CommandChannelId)}");
                }
                catch (Exception e)
                {
                    await ReplyAsync("Please provide a valid channel id");
                    Log.Error($"An error occurred:\n{e}");
                }
            }
        }
    }
}