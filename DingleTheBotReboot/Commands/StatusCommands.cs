using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Commands
{
    public class StatusCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;

        public StatusCommands(FeedbackService feedbackService, ICommandContext context)
        {
            _feedbackService = feedbackService;
            _context = context;
        }

        [Command("ping")]
        [Description("Ping the bot!")]
        public async Task<IResult> PostPongStatusAsync()
        {
            Embed embed = new Embed(Description: "Pong!", Colour: Color.Yellow);
            Result<Remora.Discord.API.Abstractions.Objects.IMessage> reply = await _feedbackService.SendContextualEmbedAsync(embed, CancellationToken);
            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
    }
}
