using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Results;
using Remora.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Services
{
    public class BotService : IHostedService
    {
        private readonly DiscordGatewayClient _discordGatewayClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly SlashService _slashService;
        private Task<Result> _clientTask;
        private readonly CancellationTokenSource _clientCancellation = new();

        public BotService(DiscordGatewayClient discordGatewayClient, ILogger<Program> logger, IConfiguration config,
            SlashService slashService)
        {
            _discordGatewayClient = discordGatewayClient;
            _logger = logger;
            _config = config;
            _slashService = slashService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Snowflake? debugServer = null;
#if DEBUG
            string debugServerString = _config["Discord:GuildId"];
            _logger.LogInformation($"Token: {_config["Discord:BotToken"]}");
            _logger.LogInformation($"Guild ID: {debugServerString}");
            if (debugServerString is not null)
            {
                if (!Snowflake.TryParse(debugServerString, out debugServer))
                {
                    _logger.LogWarning("Failed to parse guild ID.");
                    throw new Exception("Guild ID required.");
                }
            }
#endif
            Result checkSlashSupport = _slashService.SupportsSlashCommands();
            if (!checkSlashSupport.IsSuccess)
            {
                _logger.LogWarning
                (
                    "The registered commands of the bot don't support slash commands: {Reason}",
                    checkSlashSupport.Error.Message
                );
                throw new Exception("Unable to continue.");
            }
            else
            {
                Result updateSlash = await _slashService.UpdateSlashCommandsAsync(debugServer, cancellationToken);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
                    throw new Exception("Unable to continue.");
                }
            }

            _clientTask = _discordGatewayClient.RunAsync(_clientCancellation.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _clientCancellation.Cancel();
            Result runResult = await _clientTask;
            if (!runResult.IsSuccess)
            {
                switch (runResult.Error)
                {
                    case ExceptionError exe:
                        {
                            _logger.LogError
                            (
                                exe.Exception,
                                "Exception during gateway connection: {ExceptionMessage}",
                                exe.Message
                            );

                            break;
                        }
                    case GatewayDiscordError or GatewayWebSocketError:
                        {
                            _logger.LogError("Gateway error: {Message}", runResult.Error.Message);
                            break;
                        }
                    default:
                        {
                            _logger.LogError("Unknown error: {Message}", runResult.Error.Message);
                            break;
                        }
                }
            }
            _logger.LogInformation("Goodbye!");
        }
    }
}
