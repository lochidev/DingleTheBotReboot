using DingleTheBotReboot.Extensions;
using Microsoft.Extensions.Hosting;

namespace DingleTheBotReboot
{
    public class Program
    {
        public static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .CreateBotHostDefaults(botBuilder => { botBuilder.UseStartup<Startup>(); });
        }
    }
}