using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ScheduleBotAPI.Models;

namespace ScheduleBotAPI.DB
{
    public class DB
    {
        public IConfiguration AppConfiguration { get; set; }
        public DB()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            // создаем конфигурацию
            AppConfiguration = builder.Build();
        }
        public MyContext Connect()
        {
            DbContextOptionsBuilder<MyContext> optionsBuilder = new DbContextOptionsBuilder<MyContext>();
            optionsBuilder.UseSqlServer(AppConfiguration.GetConnectionString("DefaultConnection"));

            return new MyContext(optionsBuilder.Options);
        }
        public string GetConnectionString()
        {
            return AppConfiguration.GetConnectionString("DefaultConnection");
        }
    }
}