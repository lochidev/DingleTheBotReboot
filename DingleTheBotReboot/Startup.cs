using System;
using System.Net.Http;
using AniDbService;
using BanService;
using DingleTheBotReboot.Commands;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Responders;
using DingleTheBotReboot.Services;
using DingleTheBotReboot.Services.DbContext;
using DingleTheBotReboot.Services.Timer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace DingleTheBotReboot;

public class Startup
{
    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
        _config = config;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCommandGroup<StatusCommands>();
        services.AddCommandGroup<ModerationCommands>();
        services.AddCommandGroup<CoinCommands>();
        services.AddCommandGroup<AnimeCommands>();
        services.AddResponder<ButtonResponder>();
        services.AddDbContext<DingleDbContext>(options =>
            options.UseNpgsql(
                _config["Postgres"] ??
                throw new InvalidOperationException()));
        services.AddTransient<IDbContextService, DbContextService>();
        services.AddSingleton<ICommonMethodsService, CommonMethodsService>();
        services.AddGrpcClient<Banner.BannerClient>(o =>
            {
                o.Address =
                    new Uri("https://localhost:5009");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return handler;
            });
        services.AddGrpcClient<AnimeDbService.AnimeDbServiceClient>(o =>
            {
                o.Address =
                    new Uri("https://localhost:5015");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return handler;
            });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                _config["Redis"] ?? "localhost:6379"; //localhost:6379,password=redispwexample
            options.InstanceName = "DingleTheBot_";
        });
        services.AddHostedService<TimedHostedService>();
    }
}