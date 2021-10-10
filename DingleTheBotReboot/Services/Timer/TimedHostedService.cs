using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AniDbService;
using DingleTheBotReboot.Services.DbContext;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace DingleTheBotReboot.Services.Timer;

public class TimedHostedService : IHostedService
{
    private readonly ConcurrentDictionary<int, Anime> _loadedAnime;
    private readonly ILogger<TimedHostedService> _logger;
    private int _executionCount;

    public TimedHostedService(IServiceProvider services, ILogger<TimedHostedService> logger)
    {
        Services = services;
        _logger = logger;
        _loadedAnime = new ConcurrentDictionary<int, Anime>();
    }

    private IServiceProvider Services { get; }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        var operationTask = DoWorkAsync(stoppingToken);

        try
        {
            await operationTask.WaitAsync(TimeSpan.FromSeconds(1), stoppingToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        return Task.CompletedTask;
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            try
            {
                using var scope = Services.CreateScope();
                var count = Interlocked.Increment(ref _executionCount);
                if (_loadedAnime.IsEmpty || count > 20)
                {
                    var animeDbServiceClient = scope.ServiceProvider
                        .GetRequiredService<AnimeDbService.AnimeDbServiceClient>();
                    using var streamingCall = animeDbServiceClient.GetUpComingAnime(new Empty());
                    await foreach (var anime in streamingCall.ResponseStream.ReadAllAsync(
                        stoppingToken)) _loadedAnime.TryAdd(anime.AnimeId, anime);
                    Interlocked.Exchange(ref _executionCount, 0);
                }

                var (key, nearestAnime) = _loadedAnime.MinBy(x => x.Value.DateTime);
                if (nearestAnime == null) continue;
                var airingDateUtc = nearestAnime.DateTime.ToDateTime().ToUniversalTime();
                var dateTimeNowUtc = DateTime.UtcNow.AddDays(1);
                if (airingDateUtc > dateTimeNowUtc)
                {
                    await Task.Delay(airingDateUtc - dateTimeNowUtc, stoppingToken);
                    var channelApi = scope.ServiceProvider
                        .GetRequiredService<IDiscordRestChannelAPI>();
                    var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
                    var guilds = await dbContextService.GetAllGuildsAsync();
                    if (guilds is not null)
                        foreach (var guild in guilds)
                        {
                            var embed = new Embed(
                                $"Airs on {nearestAnime.DateTime.ToDateTime().ToString("D", DateTimeFormatInfo.InvariantInfo)}",
                                EmbedType.Image,
                                $"Anime name: {nearestAnime.Name}",
                                Image: new EmbedImage(nearestAnime.ImgUrl));
                            await channelApi.CreateMessageAsync(new Snowflake(guild.AnimeRemindersChannelId),
                                embeds: new List<IEmbed> {embed}, ct: stoppingToken);
                        }
                }

                _logger.LogInformation(
                    $"Timed Hosted Service is working. Successfully sent anime: {nearestAnime.AnimeId}");
                _loadedAnime.Remove(key, out _);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                _logger.LogError("AnimeDb stream canceled");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in Timed Hosted Service: {e.Message} ");
            }
    }
}