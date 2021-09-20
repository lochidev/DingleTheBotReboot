using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BanService;
using DingleTheBotReboot.Services;
using Google.Protobuf.WellKnownTypes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace DingleTheBotReboot.Commands
{
    [RequireContext(ChannelContext.Guild)]
    [Group("money")]
    [Description("Commands for all things money god")]
    public class CoinCommands : CommandGroup
    {
        private readonly Banner.BannerClient _bannerClient;
        private readonly ICommandContext _context;
        private readonly IDbContextService _dbContextService;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public CoinCommands(
            FeedbackService feedbackService,
            ICommandContext context,
            InteractionContext interactionContext,
            IDiscordRestInteractionAPI interactionApi,
            IDbContextService dbContextService, Banner.BannerClient bannerClient)
        {
            _feedbackService = feedbackService;
            _context = context;
            _interactionApi = interactionApi;
            _dbContextService = dbContextService;
            _bannerClient = bannerClient;
        }

        [Command("beg")]
        [Description("Beg the almighty for coins")]
        public async Task<IResult> Beg()
        {
            var guildId = _context.GuildID;
            if (!guildId.HasValue) return Result.FromSuccess();
            var user = _context.User;
            var isBannedResponse = await _bannerClient.IsBannedAsync(new IsBannedRequest
            {
                Identifier = user.ID.ToString(),
                ServiceId = "Dingle_Coins_Beg"
            });
            string response = null;
            if (!isBannedResponse.IsBanned)
            {
                var rnd = RandomNumberGenerator.GetInt32(1, 4);
                switch (rnd)
                {
                    case 1:
                        var coins =
                            await _dbContextService.AddCoinsAsync(user.ID.Value);
                        response = coins != 0
                            ? $"You were given {coins} coins! Guess money god has pity on ya :("
                            : null;
                        break;
                    case 2:
                        response = "Money god says no, lmfao! :D";
                        break;
                    case 3:
                        coins =
                            await _dbContextService.AddCoinsAsync(user.ID.Value, fromInclusive: 200,
                                toExclusive: 301);
                        response = coins != 0
                            ? $"You were given {coins} coins! Smh, money god simp :("
                            : null;
                        break;
                }

                await _bannerClient.BanUserAsync(new BanRequest
                {
                    Identifier = user.ID.ToString(),
                    Duration = Duration.FromTimeSpan(TimeSpan.FromMinutes(5)),
                    ServiceId = "Dingle_Coins_Beg"
                });
            }
            else
            {
                response = "5 minutes have not yet passed since you last begged, loser!";
            }

            var reply = await _feedbackService.SendContextualEmbedAsync(
                new Embed(Description: response ?? "Could not give you any coins, something went wrong :/",
                    Colour: Color.Yellow));

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
    }
}