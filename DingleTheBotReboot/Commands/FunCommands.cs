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
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    public class FunCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly IDbContextService _dbContextService;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly InteractionContext _interactionContext;

        public FunCommands(
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

        [Command("beg")]
        [Description("You may or may not get a bunch of coins :D")]
        
        public async Task<IResult> BegAsync()
        {
            var guildId = _context.GuildID;
            if (!guildId.HasValue) return Result.FromSuccess();
            var user = _context.User;
            var response =
                await _dbContextService.AddCoinsAsync(user.ID.Value);
            var reply = await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                embeds: new List<Embed>
                {
                    new(Description: response != 0 ? $"You were given {response} coins!" 
                        : "Could not give you any coins :/", Colour: Color.Yellow)
                });

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
        
    }
}