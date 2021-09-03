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
using Common.Types;
using Common;


namespace AwesomeBot.Modules
{
    [Summary("Commands for administrative action.")]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Summary("Delete messages. Must have certain permissions to use.")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task Purge(int amount)
        {
            var _user = Context.User as SocketGuildUser;
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
                await ReplyAsync($"⚖️ Deleted **_{messages.Count()}_** from **_{Context.Channel}_**");
                Console.WriteLine($"Deleted **_{messages.Count()}_** from **_{Context.Channel}_**");
        }
        [Command("mute")]
        [Summary("Mute a user in a text channel. Requires permissions")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task MuteAsync(SocketGuildUser user, int time, [Remainder]string reason = null)
        {
            var _user = Context.User as SocketGuildUser;
            var permissions = user.GetPermissions(Context.Channel as IGuildChannel);
            if (permissions.ManageChannel)
            {
                await ReplyAsync("This user is a mod and cannot be muted");
                return;
            }
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("User has higher permissions than bot");
                return;
            }
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null, false, null);

                 
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("Muted role is a higher permission than the bot");
                return;
            }
            if (user.Roles.Contains(role))
            {
                await ReplyAsync("The user is already muted");
                return;
            }
            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);

                foreach (var channel in Context.Guild.TextChannels)
                {
                    if (!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                    {

                        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));

                    }
                }
                CommandHandler.Mutes.Add(new Mute { Guild = Context.Guild, User = user, End = DateTime.Now + TimeSpan.FromMinutes(time), Role = role });
                await user.AddRoleAsync(role);
                await ReplyAsync($" ⚖️ Muted **_{user.Username}_** for **_{time} minutes_**, Reason : **_{reason ?? "None"}_**");

        }
        [Command("unmute")]
        [Summary("unmute a user")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task UnmuteAsync(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            var _user = Context.User as SocketGuildUser;
            var Permissionrole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Mod");
            if (role == null)
            {
                await ReplyAsync("This user has not been muted yet");
                return;


            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("Muted role is a higher permission than the bot");
                return;
            }
            if (!user.Roles.Contains(role))
            {
                await ReplyAsync("The user is not muted.");
                return;
            }
                await user.RemoveRoleAsync(role);
                await ReplyAsync($"⚖️ Unmuted **_{user.Username}._**");
        }
        [Command("warn")]
        [Summary("mentions a user with a warning message.")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task WarnAsync(SocketGuildUser user, [Remainder] string message = null)
        {
            var _user = Context.User as SocketGuildUser;
                await ReplyAsync($"⚖️ {user.Username} was warned. Reason: **_{message ?? "None"}_**");
        }
        [Command("kick")]
        [Summary("Kick a user from the guild.")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            var _user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Mod");
            var permissions = user.GetPermissions(Context.Channel as IGuildChannel);
            if (permissions.ManageChannel)
            {
                await ReplyAsync("This user is a mod and cannot be banned");
                return;
            }
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("This user has higher permissions than the bot");
                return;
            }
                await user.KickAsync(reason, null);
                await ReplyAsync($"⚖️ User **_{user.Username}_** was kicked for: **_{reason ?? "No reason provided"}._**");


        }
        [Command("Ban")]
        [Summary("Ban a user from the guild.")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]

        public async Task BanAsync(SocketGuildUser user, [Remainder]string reason = null)
        {
            var _user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Mod");
            var permissions = user.GetPermissions(Context.Channel as IGuildChannel);
            if (permissions.ManageChannel)
            {
                await ReplyAsync("This user is a mod and cannot be banned");
                return;
            }
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("This user has higher permissions than the bot");
                return;
            }
                await Context.Guild.AddBanAsync(user, 0, reason, null);
                await ReplyAsync($"⚖️ User **_{user.Username}_** was banned: **_{reason ?? "None"}_**");

        }
        [Command("unban")]
        [Summary("Unbans a banned user from the guild.")]
        [RequireUserPermission(ChannelPermission.ManageChannels, ErrorMessage = "Error: You do not have permission to use this command!")]
        public async Task UnbanAsync(ulong user)
        {
            var _user = Context.User as SocketGuildUser;
            var bans = Context.Guild.GetBansAsync();
            var bannedUser = bans.Result.ToList().Find(x => x.User.Id == user);
            Console.WriteLine(bannedUser);
            if (bannedUser != null)
            {
                await Context.Guild.RemoveBanAsync(user, null);
                await ReplyAsync($"⚖️ User ID **_{user}_** was unbanned.");
            }
            else
            {
                await ReplyAsync("That user is not banned");
            }
        }
    }
}
