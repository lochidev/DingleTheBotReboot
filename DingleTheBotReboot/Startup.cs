using DingleTheBotReboot.Commands;
using DingleTheBotReboot.Responders;
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
        }
    }
}