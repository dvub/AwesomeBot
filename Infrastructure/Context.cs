using Microsoft.EntityFrameworkCore;
using AwesomeBot.Core;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Context : DbContext
    {
        private readonly AppSettingsRoot _config;

        public Context(AppSettingsRoot config)
        {
            _config = config;
        }
        public DbSet<Server> Servers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _config.ConnectionString;
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            optionsBuilder.UseMySql(connectionString, serverVersion);
            
        }
        public async Task SaveServersChanges(Context context)
        {
            await context.SaveChangesAsync();
            ServerChangesSaved?.Invoke(this, EventArgs.Empty);
        }
        public EventHandler ServerChangesSaved;

    }


    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }

}
