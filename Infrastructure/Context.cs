using Microsoft.EntityFrameworkCore;
using Core;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// Context for Servers database. Contains methods to update server information.
    /// </summary>
    public class Context : DbContext
    {
        //NOTE TO SELF:
        /* when migrating or updating database, remove appsettingsroot 
         * and set connectionstring to a string
         * EFC will break otherwise
         * because ???? 
         * (thanks again huebyte)
         */

        //dependency injection
        private readonly AppSettingsRoot _config;

        public Context(AppSettingsRoot config)

        {
            _config = config;
        }

        public DbSet<Server> Servers
        {
            get;
            set;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //using appsettingsroot, get connection string
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

}