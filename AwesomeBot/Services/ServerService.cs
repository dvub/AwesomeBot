using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AwesomeBot.Services
{
    public class ServerService
    {
        public List<Server> servers;
        private Servers _servers;
        private readonly Context _context;

        public ServerService(Context context, Servers _Servers)
        {
            _context = context;
            _context.ServerChangesSaved += _context_SavedChanges;
            servers = _context.Servers.ToList();
            _servers = _Servers;
        }

        private void _context_SavedChanges(object sender, EventArgs e)
        {
            Console.WriteLine("Done!");
            servers = _context.Servers.ToList();
        }

        public async Task UpdatePrefix(ulong id, string prefix)
        {
            await _servers.ModifyGuildPrefix(id, prefix);
        }
    }
}
