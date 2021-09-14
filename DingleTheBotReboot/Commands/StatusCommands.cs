using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    public class StatusCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;

        public StatusCommands(FeedbackService feedbackService, ICommandContext context)
        {
            _feedbackService = feedbackService;
            _context = context;
        }

        [Command("ping")]
        [Description("Check whether the bot is up!")]
        public async Task<IResult> PostPongStatusAsync()
        {
            var embed = new Embed(Description: "Pong!", Colour: Color.Yellow);
            var reply = await _feedbackService.SendContextualEmbedAsync(embed, CancellationToken);
            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
    }
}