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
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    public class ModerationCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly InteractionContext _interactionContext;
        private readonly IDbContextService _dbContextService;
        public ModerationCommands(
            FeedbackService feedbackService,
            ICommandContext context,
            InteractionContext interactionContext,
            IDiscordRestInteractionAPI interactionApi, 
            IDbContextService dbContextService)
        {
            _feedbackService = feedbackService;
            _context = context;
            _interactionContext = interactionContext;
            _interactionApi = interactionApi;
            _dbContextService = dbContextService;
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
            [Description("Send the response ephemerally")] bool ephemeral = false)
        {
            var respondDeferred = await _interactionApi.CreateInteractionResponseAsync
            (
                _interactionContext.ID,
                _interactionContext.Token,
                new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage)
            );
            if (!respondDeferred.IsSuccess) return respondDeferred;
            var guildId = _context.GuildID;
            if(!guildId.HasValue)
            {
                return  Result.FromSuccess();
            }

            var response = await _dbContextService.UpdateVerificationRoleAsync(_context.GuildID.Value.Value, role.ID.Value);
            var reply = await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                embeds: new List<Embed>()
                {
                    new (Description:   response ? "All set!" : "Could not set role!", Colour: Color.Yellow)
                });

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
    }
}