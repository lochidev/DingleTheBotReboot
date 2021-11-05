﻿using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DingleTheBotReboot.Helpers;
using DingleTheBotReboot.Services.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace DingleTheBotReboot.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder CreateBotHostDefaults(this IHostBuilder builder, Action<BotHostBuilder> configure)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
        configBuilder.AddEnvironmentVariables();
#if DEBUG
        builder.UseEnvironment(Environments.Development);
        configBuilder.AddJsonFile("appsettings.development.json");
        try
        {
            configBuilder.AddUserSecrets<Program>();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
#endif
        var config = configBuilder.Build();
        builder.ConfigureAppConfiguration((context, appConfig) =>
        {
            appConfig.Sources.Clear();
            appConfig.AddConfiguration(config);
            if (!context.HostingEnvironment.IsProduction()) return;
            try
            {
                // Create a new secret client using the default credential from Azure.Identity using environment
                // variables previously set, including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
                var client = new SecretClient(new Uri($"https://{config["KeyVaultName"]}.vault.azure.net/"),
                    new DefaultAzureCredential());
                appConfig.AddAzureKeyVault(client, new KeyVaultSecretManager());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
        var botToken = config["DBOTTOKEN"];
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