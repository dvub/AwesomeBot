using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using AwesomeBot.Services;
using System.Reflection;
using System.Text.RegularExpressions;
using Infrastructure;

namespace AwesomeBot.Modules
{
    [Summary("get information!")]
    public class General : ModuleBase<SocketCommandContext>
    {
        CommandService _commandService;
        private readonly ServerService _servers;
        string timeFormat = "MM/dd/yyyy";

        public General(CommandService commandService, ServerService serverService)
        {
            _commandService = commandService;
            _servers = serverService;
        }

        
        [Command("help")]
        [Alias("commands")]
        [Summary("Get information on one or list all commands and their summaries.")]
        public async Task Help(string moduleName = null, string commandName = null)
        {
            List<ModuleInfo> modules = _commandService.Modules.ToList();

            string symbol = "";
            if (moduleName == null)
            {

                EmbedBuilder builder = new EmbedBuilder()
                    
                    .WithDescription("Displays commands and what they do.");
                
                foreach (ModuleInfo _module in modules)
                {
                    if (_module.Name == "General") symbol = "📈";
                    if (_module.Name == "Admin") symbol = "⚖️";
                    if (_module.Name == "Media") symbol = "🎶";
                    string embedFieldText = _module.Summary ?? "\n No information available for this command";
                    builder.AddField(symbol + " " + _module.Name, embedFieldText);
                }
                var embed = builder.Build();

                await ReplyAsync(null, false, embed);

            }
            else
            {
                var module = modules[modules.FindIndex(x => x.Name == moduleName)];
                if (commandName == null)
                {
                    if (modules.Contains(module))
                    {
                        if (module.Name == "General") symbol = "📈";
                        if (module.Name == "Admin") symbol = "⚖️";
                        if (module.Name == "Media") symbol = "🎶";
                        EmbedBuilder builder = new EmbedBuilder()
                            .AddField(symbol + " " + moduleName, module.Summary)
                            .AddField("_Commands:_", string.Join(", ", module.Commands.ToList()
                                .Select(x => x.Name)));
                                
                        var embed = builder.Build();
                        await ReplyAsync(null, false, embed);

                    }
                    else
                    {
                        await ReplyAsync("that module does not exist.");
                    }
                }
                else
                {
                    var command = module.Commands[module.Commands.ToList().FindIndex(x => x.Name == commandName)];
                    if (module.Commands.ToList().Contains(command))
                    {
                        if (module.Name == "General") symbol = "📈";
                        if (module.Name == "Admin") symbol = "⚖️";
                        if (module.Name == "Media") symbol = "🎶";
                        EmbedBuilder builder = new EmbedBuilder()
                            .AddField(symbol + " " + commandName, command.Summary)
                            .AddField("_Aliases:_", string.Join(", ", command.Aliases.ToList()));
                        await ReplyAsync(null, false, builder.Build());
                    }
                    else
                    {
                        await ReplyAsync("that command does not exist.");
                    }
                }


            }

        }

        
        [Command("ping")]
        [Summary("Pong!")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
            
        }


        [Command("userinfo")]
        [Alias("userinformation")]
        [Summary("Get information about a user. Leave blank for yourself or mention someone for info on them.")]
        public async Task Info(SocketGuildUser user = null)
        {
            
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("📈 " + "some information about yourself.")
                    .WithColor(new Color(255, 255, 255))
                    .WithCurrentTimestamp()
                    .AddField("User ID", Context.User.Id, true)
                    .AddField("Discriminator (#)", Context.User.Discriminator, true)
                    .AddField("Account Creation Date", Context.User.CreatedAt.ToString(timeFormat), true)
                    .AddField("Joined Server At", (Context.User as SocketGuildUser).JoinedAt.Value.ToString(timeFormat), true)
                    .AddField("Roles", string.Join(", ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)));
                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithDescription($"📈 " + "some information about {user.Username}.")
                    .WithColor(new Color(255, 255, 255))
                    .WithCurrentTimestamp()
                    .AddField("User ID", user.Id, true)
                    .AddField("Discriminator (#)", user.Discriminator, true)
                    .AddField("Account Creation Date", user.CreatedAt.ToString(timeFormat), true)
                    .AddField("Joined Server At", (user as SocketGuildUser).JoinedAt.Value.ToString(timeFormat), true)
                    .AddField("Roles", string.Join(", ", (user as SocketGuildUser).Roles.Select(x => x.Mention)));
                var embed = builder.Build();
                
                await ReplyAsync(null, false, embed);
            }
        }
        [Command("serverinfo")]
        [Alias("serverinformation")]
        [Summary("Get information for the server.")]
        public async Task ServerInformation()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("📈 " + "This message shows you some information about the server.")
                .AddField("Server Creation Date", Context.Guild.CreatedAt.ToString(timeFormat), true)
                .AddField("All Members", (Context.Guild as SocketGuild).MemberCount, true)
                .AddField("Online Members", (Context.Guild as SocketGuild).Users.Where(x => x.Status == UserStatus.Online).Count(), true); 
            var embed = builder.Build();

            await ReplyAsync(null, false, embed);

        }
        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
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
            await _servers.UpdatePrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been changed to `{prefix}`");

            
        }

    } 
}
