using Microsoft.EntityFrameworkCore;
using AwesomeBot.Core;

namespace Infrastructure
{
    public class Context : DbContext
    {

        public DbSet<Server> Servers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appsettings = AppSettingsRoot.IsCreated
                ? AppSettingsRoot.Load()
                : AppSettingsRoot.Create();
            string connectionString = appsettings.ConnectionString;
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
