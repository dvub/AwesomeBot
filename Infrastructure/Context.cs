using Microsoft.EntityFrameworkCore;
using AwesomeBot.Core;

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
        
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }

    }
}
