

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;
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
    public class CoinCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly IDbContextService _dbContextService;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly InteractionContext _interactionContext;

        public CoinCommands(
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
        [Description("Beg the almighty for coins")]
        public async Task<IResult> Beg()
        {
            // This is something we're supposed to handle
            var respondDeferred = await _interactionApi.CreateInteractionResponseAsync
            (
                _interactionContext.ID,
                _interactionContext.Token,
                new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage)
            );
            if (!respondDeferred.IsSuccess) return respondDeferred;
            var guildId = _context.GuildID;
            if (!guildId.HasValue) return Result.FromSuccess();
            var user = _context.User;
            var rnd = RandomNumberGenerator.GetInt32(1, 4);
            string response = null;
            switch (rnd)
            {
                case 1:
                    var coins =
                        await _dbContextService.AddCoinsAsync(user.ID.Value);
                    response = coins != 0
                        ? $"You were given {coins} coins! Guess money god has pity on ya :(" : null;
                    break;
                case 2:
                    response = "Money god says no, lmfao! :D";
                    break;
                case 3:
                    coins =
                        await _dbContextService.AddCoinsAsync(user.ID.Value, fromInclusive: 200, 
                            toExclusive: 301);
                    response = coins != 0
                        ? $"You were given {coins} coins! Smh, money god simp :(" : null;
                    break;
            }
            
            var reply = await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                embeds: new List<Embed>
                {
                    new(Description: response ?? "Could not give you any coins, something went wrong :/", Colour: Color.Yellow)
                });

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
        
    }
}