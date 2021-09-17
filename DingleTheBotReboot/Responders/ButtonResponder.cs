using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using DingleTheBotReboot.Services;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace DingleTheBotReboot.Responders
{
    public class ButtonResponder : IResponder<IInteractionCreate>
    {
        private readonly IDbContextService _dbContextService;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public ButtonResponder(IDiscordRestGuildAPI guildApi,
            IDiscordRestInteractionAPI interactionApi, IDbContextService dbContextService)
        {
            _guildApi = guildApi;
            _interactionApi = interactionApi;
            _dbContextService = dbContextService;
        }

        public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent,
            CancellationToken ct = new())
        {
            if (gatewayEvent.Type != InteractionType.MessageComponent) return Result.FromSuccess();

            var data = gatewayEvent.Data.Value;
            var type = data.ComponentType.Value;

            if (type != ComponentType.Button) return Result.FromSuccess();
            // This is something we're supposed to handle
            var respondDeferred = await _interactionApi.CreateInteractionResponseAsync
            (
                gatewayEvent.ID,
                gatewayEvent.Token,
                new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage),
                ct
            );

            if (!respondDeferred.IsSuccess) return respondDeferred;

            var user = gatewayEvent.User.HasValue
                ? gatewayEvent.User.Value
                : gatewayEvent.Member.HasValue
                    ? gatewayEvent.Member.Value.User.HasValue
                        ? gatewayEvent.Member.Value.User.Value
                        : null
                    : null;
            if (user is null || !gatewayEvent.GuildID.HasValue) return Result.FromSuccess();
            var guildId = gatewayEvent.GuildID.Value;
            var userId = user.ID;
            var buttonNonce = data.CustomID.Value ?? throw new InvalidOperationException();
            if (buttonNonce == "verifyBtn")
            {
                var storedGuild = await _dbContextService.GetGuildAsync(guildId.Value);
                var found = storedGuild is not null;
                if (found)
                    await _guildApi.AddGuildMemberRoleAsync(
                        guildId,
                        userId,
                        new Snowflake(storedGuild.VerificationRoleId), ct: ct);
                var embed = new List<IEmbed>
                {
                    new Embed(
                        Description: found ? "You were verified!" : "No verification role has been set for this server",
                        Colour: Color.Yellow)
                };
                var reply = await _interactionApi.CreateFollowupMessageAsync(
                    gatewayEvent.ApplicationID,
                    gatewayEvent.Token,
                    embeds: embed,
                    flags: found ? MessageFlags.Ephemeral : default(Optional<MessageFlags>),
                    ct: ct);
                return !reply.IsSuccess
                    ? Result.FromError(reply)
                    : Result.FromSuccess();
            }

            return Result.FromSuccess();
        }
    }
}