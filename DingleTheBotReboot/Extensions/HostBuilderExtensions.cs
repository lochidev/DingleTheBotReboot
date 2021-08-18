using DingleTheBotReboot.Helpers;
using DingleTheBotReboot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using System;

namespace DingleTheBotReboot.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder CreateBotHostDefaults(this IHostBuilder builder, Action<BotHostBuilder> configure)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
#if DEBUG
            builder.UseEnvironment(Environments.Development);
            configBuilder.AddJsonFile("appsettings.development.json");
            try
            {
                configBuilder.AddUserSecrets<Program>();
            }
            catch (InvalidOperationException) { }
#endif
            configBuilder.AddEnvironmentVariables();
            IConfigurationRoot config = configBuilder.Build();
            builder.ConfigureAppConfiguration(appConfig =>
            {
                appConfig.Sources.Clear();
                appConfig.AddConfiguration(config);
            });

            builder.ConfigureServices(serviceCollection =>
            {
                serviceCollection
                    .AddDiscordGateway(_ => config["Discord:BotToken"])
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
