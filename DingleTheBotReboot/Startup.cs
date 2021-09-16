using System;
using DingleTheBotReboot.Commands;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Responders;
using DingleTheBotReboot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace DingleTheBotReboot
{
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
            services.AddResponder<ButtonResponder>();
            services.AddDbContext<DingleDbContext>(options =>
                options.UseCosmos(
                    _config["Cosmos_AccountEndpoint"] ??
                    throw new InvalidOperationException(),
                    _config["Cosmos_AccountKey"] ??
                    throw new InvalidOperationException(),
                    _config["Cosmos_Database"] ??
                    throw new InvalidOperationException()));
            services.AddTransient<IDbContextService, DbContextService>();
        }
    }
}