using Microsoft.EntityFrameworkCore;
using AwesomeBot.Core;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Context : DbContext
    {
        //NOTE TO SELF:
        /* when migrating or updating database, remove appsettingsroot 
         * and set connectionstring to a string
         * EFC will break otherwise
         * because ???? 
         * (thanks again huebyte)
            */
       private readonly AppSettingsRoot _config;

        public Context(AppSettingsRoot config)
            
        {
           _config = config;
        }

        public DbSet<Server> Servers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "server=localhost;user=root;database=awesomebot;port=3306;Connection Timeout=5;"; //"server =localhost;user=root;database=awesomebot;port=3306;Connection Timeout=5;"
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
