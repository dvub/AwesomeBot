using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure
{
    public class Context : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "server=localhost;user=root;database=awesomebot;port=3306;Connection Timeout=5;";
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            optionsBuilder.UseMySql(connectionString, serverVersion);
        }
        
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }

    }
}
