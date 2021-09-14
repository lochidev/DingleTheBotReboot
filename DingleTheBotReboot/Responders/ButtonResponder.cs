using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public ButtonResponder(IDiscordRestGuildAPI guildApi,
            IDiscordRestInteractionAPI interactionApi)
        {
            _guildApi = guildApi;
            _interactionApi = interactionApi;
        }

        public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent,
            CancellationToken ct = new())
        {
            if (gatewayEvent.Type != InteractionType.MessageComponent) return Result.FromSuccess();

            var data = gatewayEvent.Data.Value ?? throw new InvalidOperationException();
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

            if (user is null) return Result.FromSuccess();

            var userId = user.ID;

            var buttonNonce = data.CustomID.Value ?? throw new InvalidOperationException();
            if (buttonNonce == "verifyBtn")
            {
                await _guildApi.AddGuildMemberRoleAsync(
                    gatewayEvent.GuildID.Value,
                    userId,
                    new Snowflake(787146778357137430), ct: ct);
                var embed = new List<IEmbed>
                {
                    new Embed(Description: "You were verified!", Colour: Color.Yellow)
                };
                var reply = await _interactionApi.CreateFollowupMessageAsync(
                    gatewayEvent.ApplicationID,
                    gatewayEvent.Token,
                    embeds: embed,
                    flags: MessageFlags.Ephemeral,
                    ct: ct);
                return !reply.IsSuccess
                    ? Result.FromError(reply)
                    : Result.FromSuccess();
            }

            return Result.FromSuccess();
        }
    }
}