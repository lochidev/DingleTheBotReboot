using System;
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
    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<int, Anime> _loadedAnime;
    private readonly ILogger<TimedHostedService> _logger;
    private Task _timerTask;

    public TimedHostedService(IServiceProvider services, ILogger<TimedHostedService> logger)
    {
        Services = services;
        _logger = logger;
        _loadedAnime = new Dictionary<int, Anime>();
    }

    private IServiceProvider Services { get; }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        _timerTask = DoWorkAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _cts.Cancel();

        await _timerTask;

        _cts.Dispose();

        _logger.LogInformation("Timed Hosted Service is stopped.");
    }

    private async Task DoWorkAsync()
    {
        try
        {
            var nextRefreshDate = DateTime.UtcNow.AddDays(4);
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
            while (await timer.WaitForNextTickAsync(_cts.Token))
                try
                {
                    var dateTimeNowUtc = DateTime.UtcNow;
                    if (_loadedAnime.Count == 0 || dateTimeNowUtc > nextRefreshDate)
                    {
                        nextRefreshDate = dateTimeNowUtc.AddDays(4);
                        using var scope = Services.CreateScope();
                        var animeDbServiceClient = scope.ServiceProvider
                            .GetRequiredService<AnimeDbService.AnimeDbServiceClient>();
                        using var streamingCall = animeDbServiceClient.GetUpComingAnime(new Empty());
                        await foreach (var anime in streamingCall.ResponseStream.ReadAllAsync(
                                           _cts.Token)) _loadedAnime.TryAdd(anime.AnimeId, anime);
                    }

                    var (key, nearestAnime) = _loadedAnime.MinBy(x => x.Value.DateTime);
                    if (nearestAnime == null) continue;
                    var airingDateUtc = nearestAnime.DateTime.ToDateTime().ToUniversalTime();
                    if (airingDateUtc >= dateTimeNowUtc)
                    {
                        if(airingDateUtc != dateTimeNowUtc)
                            await Task.Delay(airingDateUtc - dateTimeNowUtc, _cts.Token);
                        using var scope = Services.CreateScope();
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
                                    embeds: new List<IEmbed> {embed}, ct: _cts.Token);
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
                    _logger.LogError($"Error in Timed Hosted Service (Worker): {e.Message} ");
                }
        }
        catch (OperationCanceledException)
        {
            //ignore
        }
    }
}