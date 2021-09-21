using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using DingleTheBotReboot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    [RequireContext(ChannelContext.Guild)]
    [Group("mod")]
    [Description("Commands for moderation")]
    public class ModerationCommands : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommonMethodsService _commonMethodsService;
        private readonly ICommandContext _context;
        private readonly IDbContextService _dbContextService;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly InteractionContext _interactionContext;

        public ModerationCommands(
            FeedbackService feedbackService,
            ICommandContext context,
            InteractionContext interactionContext,
            IDiscordRestInteractionAPI interactionApi,
            IDbContextService dbContextService,
            IDiscordRestGuildAPI guildApi,
            IDiscordRestChannelAPI channelApi,
            ICommonMethodsService commonMethodsService,
            IDiscordRestWebhookAPI webhookApi)
        {
            _feedbackService = feedbackService;
            _context = context;
            _interactionContext = interactionContext;
            _interactionApi = interactionApi;
            _dbContextService = dbContextService;
            _guildApi = guildApi;
            _channelApi = channelApi;
            _commonMethodsService = commonMethodsService;
        }

        [Command("sendverifymessage")]
        [Description("Sends a button that users can use to get the specified role")]
        public async Task<IResult> SendVerifyMessageAsync()
        {
            var reply = await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                "Hold your horses! Are you not a bot? Prove it!",
                components: new List<ActionRowComponent>
                {
                    new(new List<ButtonComponent>
                    {
                        new(ButtonComponentStyle.Primary, "👌 Let me in already! :D", CustomID: "verifyBtn")
                    })
                });

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }

        [Command("setverificationrole")]
        [Description("Sets the role that dingle uses to give when users verify in your server")]
        public async Task<IResult> SetVerificationRole(
            [Description("The role to set")] IRole role,
            [Description("Send the response ephemerally")]
            bool ephemeral = false)
        {
            var guildId = _context.GuildID;
            if (!guildId.HasValue) return Result.FromSuccess();

            var response =
                await _dbContextService.UpdateVerificationRoleAsync(guildId.Value.Value, role.ID.Value);
            var reply = await _feedbackService.SendContextualEmbedAsync(
                new Embed(Description: response ? "All set!" : "Could not set role!",
                    Colour: Color.Yellow));

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }

        [Command("nuke")]
        [Description(
            "WARNING: Deletes the current channel and creates a duplicate!")]
        [RequireDiscordPermission(DiscordStagePermission.ManageChannels)]
        public async Task<IResult> Nuke([Description("Confirmation needed")] bool confirm)
        {
            var guildId = _context.GuildID;
            if (!guildId.HasValue) return Result.FromSuccess();
            if (!confirm)
            {
                var user = _context.User;
                var guild = (await _guildApi.GetGuildAsync(guildId.Value)).Entity;
                var ownerId = guild.OwnerID.Value;
                string response = null;
                if (ownerId != user.ID.Value) return Result.FromSuccess();
                var newChannelId = await _commonMethodsService.NukeChannelAsync(_guildApi, _channelApi,
                    _context.ChannelID,
                    guildId.Value, user);
                var nuked = newChannelId != 0;
                response = nuked
                    ? $"Channel nuked by <@{ownerId}>! https://media.giphy.com/media/HhTXt43pk1I1W/giphy.gif?cid=ecf05e4748fpov1bzrxehcmgt8ldeti17pdxk0smym4odqd3&rid=giphy.gif&ct=g"
                    : "Could not nuke https://media.giphy.com/media/l0HlRT5Fq5Te1kJKU/giphy.gif?cid=ecf05e472afspgurxzkdu75oxr5mitius1inno4iuj5gh48k&rid=giphy.gif&ct=g";
                var reply = await _channelApi.CreateMessageAsync(new Snowflake(newChannelId), response);
                return !reply.IsSuccess
                    ? Result.FromError(reply)
                    : Result.FromSuccess();
            }
            else
            {
                var reply = await _interactionApi.CreateFollowupMessageAsync(
                    _interactionContext.ApplicationID,
                    _interactionContext.Token,
                    "Please confirm nuke to oblivion. Come on mr/mrs owner! https://media.giphy.com/media/3orieUjkio8buYT5ss/giphy.gif?cid=790b761120202c9256ef10b18999061d868cc6debb216b2d&rid=giphy.gif&ct=g",
                    components: new List<ActionRowComponent>
                    {
                        new(new List<ButtonComponent>
                        {
                            new(ButtonComponentStyle.Danger, "👌 Nuke it! :D", CustomID: "nuke")
                        })
                    });
                return !reply.IsSuccess
                    ? Result.FromError(reply)
                    : Result.FromSuccess();
            }
        }
    }
}