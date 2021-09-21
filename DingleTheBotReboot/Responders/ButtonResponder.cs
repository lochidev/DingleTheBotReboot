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
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommonMethodsService _commonMethodsService;
        private readonly IDbContextService _dbContextService;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public ButtonResponder(IDiscordRestGuildAPI guildApi,
            IDiscordRestInteractionAPI interactionApi,
            IDbContextService dbContextService,
            ICommonMethodsService commonMethodsService,
            IDiscordRestChannelAPI channelApi)
        {
            _guildApi = guildApi;
            _interactionApi = interactionApi;
            _dbContextService = dbContextService;
            _commonMethodsService = commonMethodsService;
            _channelApi = channelApi;
        }

        public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent,
            CancellationToken ct = new())
        {
            if (gatewayEvent.Type != InteractionType.MessageComponent) return Result.FromSuccess();

            var data = gatewayEvent.Data.Value;
            var type = data.ComponentType.Value;

            if (type != ComponentType.Button) return Result.FromSuccess();
            // // This is something we're supposed to handle
            // var respondDeferred = await _interactionApi.CreateInteractionResponseAsync
            // (
            //     gatewayEvent.ID,
            //     gatewayEvent.Token,
            //     new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage),
            //     ct
            // );
            //
            // if (!respondDeferred.IsSuccess) return respondDeferred;

            var user = gatewayEvent.User.HasValue
                ? gatewayEvent.User.Value
                : gatewayEvent.Member.HasValue
                    ? gatewayEvent.Member.Value.User.HasValue
                        ? gatewayEvent.Member.Value.User.Value
                        : null
                    : null;
            if (user is null || !gatewayEvent.GuildID.HasValue || !gatewayEvent.ChannelID.HasValue)
                return Result.FromSuccess();
            var guildId = gatewayEvent.GuildID.Value;
            var userId = user.ID;
            var buttonNonce = data.CustomID.Value ?? throw new InvalidOperationException();
            switch (buttonNonce)
            {
                case "verifyBtn":
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
                            Description: found
                                ? "You were verified!"
                                : "No verification role has been set for this server",
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
                case "nuke":
                    var guild = (await _guildApi.GetGuildAsync(guildId, ct: ct)).Entity;
                    var ownerId = guild.OwnerID.Value;
                    string response = null;
                    if (ownerId != user.ID.Value) return Result.FromSuccess();
                    var newChannelId = await _commonMethodsService.NukeChannelAsync(_guildApi, _channelApi,
                        gatewayEvent.ChannelID.Value, guildId, user);
                    var nuked = newChannelId != 0;
                    response = nuked
                        ? $"Channel nuked by <@{ownerId}>! https://media.giphy.com/media/HhTXt43pk1I1W/giphy.gif?cid=ecf05e4748fpov1bzrxehcmgt8ldeti17pdxk0smym4odqd3&rid=giphy.gif&ct=g"
                        : "Could not nuke https://media.giphy.com/media/l0HlRT5Fq5Te1kJKU/giphy.gif?cid=ecf05e472afspgurxzkdu75oxr5mitius1inno4iuj5gh48k&rid=giphy.gif&ct=g";
                    reply = await _channelApi.CreateMessageAsync(new Snowflake(newChannelId),
                        response, ct: ct);
                    return !reply.IsSuccess
                        ? Result.FromError(reply)
                        : Result.FromSuccess();
            }

            return Result.FromSuccess();
        }
    }
}