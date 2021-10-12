using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using AniDbService;
using DingleTheBotReboot.Services.DbContext;
using Grpc.Core;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace DingleTheBotReboot.Commands;

[RequireContext(ChannelContext.Guild)]
[Group("anime")]
[Description("Commands for anime")]
public class AnimeCommands : CommandGroup
{
    private readonly AnimeDbService.AnimeDbServiceClient _animeDbServiceClient;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _context;
    private readonly IDbContextService _dbContextService;
    private readonly FeedbackService _feedbackService;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly InteractionContext _interactionContext;

    public AnimeCommands(InteractionContext interactionContext, IDiscordRestInteractionAPI interactionApi,
        IDiscordRestGuildAPI guildApi, FeedbackService feedbackService, IDbContextService dbContextService,
        ICommandContext context,
        IDiscordRestChannelAPI channelApi,
        AnimeDbService.AnimeDbServiceClient animeDbServiceClient)
    {
        _interactionContext = interactionContext;
        _interactionApi = interactionApi;
        _guildApi = guildApi;
        _feedbackService = feedbackService;
        _dbContextService = dbContextService;
        _context = context;
        _channelApi = channelApi;
        _animeDbServiceClient = animeDbServiceClient;
    }

    [Command("setreminders")]
    [Description("Set this channel to get anime reminders to this channel")]
    public async Task<IResult> SetRemindersAsync()
    {
        var guildId = _context.GuildID;
        var channelId = _context.ChannelID;
        if (!guildId.HasValue) return Result.FromSuccess();

        var success = await _dbContextService.UpdateAnimeChannelAsync(guildId.Value.Value, channelId.Value);
        var responseStr = success
            ? "This channel is set to receive anime updates for your guild. " +
              "If you wish to see the upcoming anime now, please use ```/anime current```"
            : "Could not set channel for reminders";
        var reply = await _interactionApi.CreateFollowupMessageAsync(
            _interactionContext.ApplicationID,
            _interactionContext.Token,
            responseStr
        );

        return !reply.IsSuccess
            ? Result.FromError(reply)
            : Result.FromSuccess();
    }

    [Command("current")]
    [Description("Gets the upcoming anime currently loaded by the bot, source: anidb.net")]
    public async Task<IResult> GetCurrentAsync()
    {
        var guildId = _context.GuildID;
        var channelId = _context.ChannelID;
        if (!guildId.HasValue) return Result.FromSuccess();
        var loadedAnime = new HashSet<Anime>();
        using var streamingCall = _animeDbServiceClient.GetUpComingAnime(new Empty());
        await foreach (var anime in streamingCall.ResponseStream.ReadAllAsync())
            loadedAnime.Add(anime);
        List<Embed> embeds = new();
        foreach (var anime in loadedAnime)
        {
            var embed = new Embed(
                $"Airs on {anime.DateTime.ToDateTime().ToString("D", DateTimeFormatInfo.InvariantInfo)}",
                EmbedType.Image,
                $"Anime name: {anime.Name}",
                Image: new EmbedImage(anime.ImgUrl));
            embeds.Add(embed);
            if (embeds.Count != 10) continue;
            await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                embeds: embeds);
            embeds.Clear();
        }

        if (embeds.Count > 0)
            await _interactionApi.CreateFollowupMessageAsync(
                _interactionContext.ApplicationID,
                _interactionContext.Token,
                embeds: embeds);


        return Result.FromSuccess();
    }
}