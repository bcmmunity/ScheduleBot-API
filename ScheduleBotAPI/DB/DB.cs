using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ScheduleBotAPI.Models;

namespace ScheduleBotAPI.DB
{
    public class DB
    {
        public static bool IsDefault = true;
        public IConfiguration AppConfiguration { get; set; }
        public DB()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            // создаем конфигурацию
            AppConfiguration = builder.Build();
        }
        public string GetConnectionString()
        {
            if(IsDefault)
                return AppConfiguration.GetConnectionString("DefaultConnection");
            else
                return AppConfiguration.GetConnectionString("BetaConnection");
        }
    }
}