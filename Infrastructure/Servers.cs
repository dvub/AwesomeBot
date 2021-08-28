﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace Infrastructure
{
    public class Servers
    {
        private readonly Context _context;
        
        public Servers(Context context)
        {
            _context = context;
        }
        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers
                .FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server { Id = id, Prefix = prefix });

            }
            else
            {
                server.Prefix = prefix;

            }
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers
                .Where(x => x.Id == id)
                .Select(x => x.Prefix)
                .FirstOrDefaultAsync();

            return await Task.FromResult(prefix);
        }
    }

}
