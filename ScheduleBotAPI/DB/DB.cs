using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ScheduleBotAPI.Models;

namespace ScheduleBotAPI.DB
{
    public class DB
    {
        public static bool IsDefault = true;
        public static IConfiguration AppConfiguration { get; set; }
        public static string GetConnectionString()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            // создаем конфигурацию
            AppConfiguration = builder.Build();

            if (IsDefault)
                return AppConfiguration.GetConnectionString("DefaultConnection");
            else
                return AppConfiguration.GetConnectionString("BetaConnection");
        }
    }
}