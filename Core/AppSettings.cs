
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Core
{
    //Thanks to HueByte for letting me use his AppSettings class
    //and helping me out so much

    public class ConnectionString
    {
        public string DatabaseConnectionString { get; set; }
    }
    public class AppSettingsRoot
    {
        public string ConnectionString { get; set; }
        public string TokenString { get; set; }

        [JsonIgnore]
        public static string FILE_NAME = AppContext.BaseDirectory + "appsettings.json";

        [JsonIgnore]
        public static bool IsCreated
            => (File.Exists(FILE_NAME));

        public static AppSettingsRoot Create()
        {
            if (IsCreated)
            {
                return Load();
            }
            if (!Directory.Exists(AppContext.BaseDirectory + @"\save"))
                Directory.CreateDirectory(AppContext.BaseDirectory + @"\save");
            var config = new AppSettingsRoot()
            {
                ConnectionString = "server=;user=;database=;port=;Connection Timeout=;",
                TokenString = ""

            };
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            File.WriteAllBytes(FILE_NAME, JsonSerializer.SerializeToUtf8Bytes(config, options));
            return config;

        }
        public static AppSettingsRoot Load()
        {
            var readBytes = File.ReadAllBytes(FILE_NAME);
            var config = JsonSerializer.Deserialize<AppSettingsRoot>(readBytes);
            return config;
        }
    }
}

