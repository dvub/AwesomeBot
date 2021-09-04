using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Core;

namespace AwesomeBot.Services
{
    //This might need some work

    /// <summary>
    /// Service for making server database changes.
    /// </summary>
    public class ServerService
    {
        //dependency injection
        public List<Server> servers;
        private readonly Context _context;

        public ServerService(Context context)
        {
            _context = context;
            _context.ServerChangesSaved += _context_SavedChanges;
            servers = _context.Servers.ToList();
        }
        /// <summary>
        /// Save changes to server.
        /// </summary>
        private void _context_SavedChanges(object sender, EventArgs e)
        {
            servers = _context.Servers.ToList(); //updates server list
        }
        /// <summary>
        /// Set the guild prefix to a new string prefix.
        /// </summary>
        /// <param name="id">The server id.</param>
        /// <param name="prefix">The prefix to update to.</param>
        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
              .FindAsync(id);

            if (server == null)
            {
                //create new server if none
                _context.Add(new Server
                {
                    Id = id,
                    Prefix = prefix,
                    Greeting = "",
                    GreetingType = GreetingType.Disabled
                });

            }
            else
            {
                server.Prefix = prefix;

            }
            await _context.SaveServersChanges(_context);
        }
        /// <summary>
        /// Set the guild greeting type. 
        /// </summary>
        /// <param name="id">The server id.</param>
        /// <param name="greetingType">How the use is greeted.</param>
        /// <returns></returns>
        public async Task ModifyGuildGreetingType(ulong id, string greetingType)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                //if no server entry, make a new one
                _context.Add(new Server
                {
                    Id = id,
                    Prefix = "!",
                    Greeting = "",
                    GreetingType = (GreetingType)Enum.Parse(typeof(GreetingType), greetingType) //parse string to GreetingType enum
                });

            }
            else
            {
                server.GreetingType = (GreetingType)Enum.Parse(typeof(GreetingType), greetingType, true); //parse string to GreetingType enum

            }
            await _context.SaveServersChanges(_context);
        }
        /// <summary>
        /// Set the greeting message for the guild.
        /// </summary>
        /// <param name="id">The server id.</param>
        /// <param name="greeting">The greeting message.</param>
        /// <returns></returns>
        public async Task ModifyGuildGreeting(ulong id, string greeting)
        {
            var server = await _context.Servers.FindAsync(id);
            //if no server make a new one
            if (server == null)
            {
                _context.Add(new Server
                {
                    Id = id,
                    Prefix = "!",
                    Greeting = "",
                    GreetingType = GreetingType.Channel
                });

            }
            else
            {
                server.Greeting = greeting;

            }
            await _context.SaveServersChanges(_context);
        }
        /// <summary>
        /// Set the channel id in which users will be greeted for the guild.
        /// </summary>
        /// <param name="id">The server id.</param>
        /// <param name="channelId">The channel id where users will be greeted IF GreetingType is set to Channel.</param>
        /// <returns></returns>
        public async Task ModifyGreetingChannelId(ulong id, ulong? channelId)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server
                {
                    Id = id,
                    Prefix = "!",
                    Greeting = "",
                    GreetingType = GreetingType.Channel,
                    GreetingChannelId = channelId
                });

            }
            else
            {
                server.GreetingChannelId = channelId;

            }
            await _context.SaveServersChanges(_context);
        }
        /// <summary>
        /// Set the channel id in which users can send commands.
        /// </summary>
        /// <param name="id">The server id.</param>
        /// <param name="channelId">The channel id in which commands will be sent.</param>
        /// <returns></returns>
        public async Task ModifyCommandsChannelId(ulong id, ulong? channelId)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server
                {
                    Id = id,
                    Prefix = "!",
                    Greeting = "",
                    GreetingType = GreetingType.Channel,
                    CommandChannelId = channelId
                });

            }
            else
            {
                server.CommandChannelId = channelId;

            }
            await _context.SaveServersChanges(_context);
        }

    }
}