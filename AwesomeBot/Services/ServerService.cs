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
    public class ServerService
    {
        public List<Server> servers;
        private readonly Context _context;

        public ServerService(Context context)
        {
            _context = context;
            _context.ServerChangesSaved += _context_SavedChanges;
            servers = _context.Servers.ToList();
        }

        private void _context_SavedChanges(object sender, EventArgs e)
        {
            servers = _context.Servers.ToList();
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = prefix, Greeting = "", GreetingType = GreetingType.Disabled });

            }
            else
            {
                server.Prefix = prefix;

            }
            await _context.SaveServersChanges(_context);
        }
        public async Task ModifyGuildGreetingType(ulong id, string greetingType)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = "!", Greeting = "", GreetingType = (GreetingType)Enum.Parse(typeof(GreetingType), greetingType) });

            }
            else
            {
                server.GreetingType = (GreetingType)Enum.Parse(typeof(GreetingType), greetingType);

            }
            await _context.SaveServersChanges(_context);
        }
        public async Task ModifyGuildGreeting(ulong id, string greeting)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = "!", Greeting = "", GreetingType = GreetingType.Channel });

            }
            else
            {
                server.Greeting = greeting;

            }
            await _context.SaveServersChanges(_context);
        }
        public async Task ModifyGreetingChannelId(ulong id, ulong? channelId)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = "!", Greeting = "", GreetingType = GreetingType.Channel, GreetingChannelId = channelId });

            }
            else
            {
                server.GreetingChannelId = channelId;

            }
            await _context.SaveServersChanges(_context);
        }
        public async Task ModifyCommandsChannelId(ulong id, ulong? channelId)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = "!", Greeting = "", GreetingType = GreetingType.Channel, CommandChannelId = channelId });

            }
            else
            {
                server.CommandChannelId = channelId;

            }
            await _context.SaveServersChanges(_context);
        }

    }
}
