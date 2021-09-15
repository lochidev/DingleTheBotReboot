using System;
using DingleTheBotReboot.Helpers;
using DingleTheBotReboot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace DingleTheBotReboot.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder CreateBotHostDefaults(this IHostBuilder builder, Action<BotHostBuilder> configure)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
#if DEBUG
            builder.UseEnvironment(Environments.Development);
            configBuilder.AddJsonFile("appsettings.development.json");
            try
            {
                configBuilder.AddUserSecrets<Program>();
            }
            catch (InvalidOperationException)
            {
            }
#endif
            configBuilder.AddEnvironmentVariables();
            var config = configBuilder.Build();
            builder.ConfigureAppConfiguration(appConfig =>
            {
                appConfig.Sources.Clear();
                appConfig.AddConfiguration(config);
            });
            var botToken = config["Discord:BotToken"] ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (botToken is null) throw new Exception("Bot token not found");
            builder.ConfigureServices(serviceCollection =>
            {
                serviceCollection
                    .AddDiscordGateway(_ => botToken)
                    .AddDiscordCommands(true);
                configure.Invoke(new BotHostBuilder(config, serviceCollection));
                serviceCollection
                    .AddDiscordCaching()
                    .AddHostedService<BotService>();
            });

            return builder;
        }
    }
}