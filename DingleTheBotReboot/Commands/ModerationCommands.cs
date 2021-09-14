using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    public class ModerationCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly InteractionContext _interactionContext;

        public ModerationCommands(
            FeedbackService feedbackService,
            ICommandContext context,
            InteractionContext interactionContext,
            IDiscordRestInteractionAPI interactionApi)
        {
            _feedbackService = feedbackService;
            _context = context;
            _interactionContext = interactionContext;
            _interactionApi = interactionApi;
        }

        [Command("sendverifymessage")]
        [Description("Sends a button that users can use to get he specified role")]
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
    }
}