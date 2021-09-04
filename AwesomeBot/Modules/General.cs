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
using Common.Extensions;

namespace AwesomeBot.Modules
{
    [Summary("get information!")]
    public class General : ModuleBase<SocketCommandContext>
    {
        CommandService _commandService;
        private readonly ServerService _servers;
        string timeFormat = "MM/dd/yyyy";
        Dictionary<string, string> symbolDictionary;
        public General(CommandService commandService, ServerService serverService)
        {
            symbolDictionary = new Dictionary<string, string>();
            ConfigureDictionary(symbolDictionary);
            _commandService = commandService;
            _servers = serverService;
        }
        public void ConfigureDictionary(Dictionary<string, string> dictionary)
        {

            dictionary.Add("General", "📈");
            dictionary.Add("Admin", "⚖️");
            dictionary.Add("Media", "🎶");
            dictionary.Add("Configuration", "🔧");
        }

        [Command("help")]
        [Alias("commands")]
        [Summary("Get information on one or list all commands and their summaries.")]
        public async Task Help(string moduleName = null, string commandName = null)
        {
            List<ModuleInfo> modules = _commandService.Modules.ToList();
            var prefix = _servers.servers.Find(x => x.Id == Context.Guild.Id).Prefix;
            if (moduleName == null)
            {

                EmbedBuilder builder = new EmbedBuilder()

                    .WithDescription("Displays commands and what they do.");

                foreach (ModuleInfo _module in modules)
                {
                    string embedFieldText = _module.Summary.CapitalizeFirst() ?? "\n No information available for this command";
                    builder.AddField(symbolDictionary[_module.Name] + " " + _module.Name, embedFieldText);
                }
                var embed = builder.Build();

                await ReplyAsync(null, false, embed);

            }
            else
            {
                var module = modules.Find(x => x.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase));
                if (commandName == null)
                {
                    if (modules.Contains(module))
                    {

                        EmbedBuilder builder = new EmbedBuilder()
                            .AddField(symbolDictionary[module.Name] + " " + moduleName, module.Summary.CapitalizeFirst())

                            .AddField("_Commands:_", string.Join($", ", module.Commands.ToList().Select(x => $"`{x.Name}`").ToList().LowerStringList()));


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
                    var command = module.Commands.ToList().Find(x => x.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));
                    if (module.Commands.ToList().Contains(command))
                    {

                        EmbedBuilder builder = new EmbedBuilder()
                            .AddField(symbolDictionary[module.Name] + " " + commandName, command.Summary.CapitalizeFirst())
                            .AddField("_Aliases:_", string.Join($", ", command.Aliases.ToList().LowerStringList()))
                            .AddField("_Usage:_", $"`{prefix}{command.Name} {string.Join(" ", command.Parameters.ToList().Select(x => $"<{x.Name}>"))}`");
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

    }
}
