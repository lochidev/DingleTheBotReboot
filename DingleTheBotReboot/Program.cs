using System;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DingleTheBotReboot;

public class Program
{
    public static void Main()
    {
        var host = CreateHostBuilder().Build();
            CreateDbIfNotExists(host);
            host.Run();
    }

    private static void CreateDbIfNotExists(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<DingleDbContext>();
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB");
        }
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .CreateBotHostDefaults(botBuilder => { botBuilder.UseStartup<Startup>(); });
    }
}