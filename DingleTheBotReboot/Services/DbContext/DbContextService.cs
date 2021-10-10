using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Extensions;
using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DingleTheBotReboot.Services.DbContext;

public class DbContextService : IDbContextService
{
    private readonly DingleDbContext _dingleDbContext;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DbContextService> _logger;

    public DbContextService(DingleDbContext dingleDbContext, ILogger<DbContextService> logger, Random rnd,
        IDistributedCache distributedCache)
    {
        _dingleDbContext = dingleDbContext;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<bool> UpdateVerificationRoleAsync(ulong guildId, ulong verificationRoleId)
    {
        try
        {
            var guild = await GetGuildAsync(guildId);
            if (guild is null)
            {
                await _dingleDbContext.Guilds.AddAsync(new Guild
                {
                    GuildId = guildId,
                    VerificationRoleId = verificationRoleId
                });
            }
            else
            {
                guild.VerificationRoleId = verificationRoleId;
                _dingleDbContext.Update(guild);
            }

            var rows = await _dingleDbContext.SaveChangesAsync();
            return rows != 0;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when setting verification role: {Message}", e.Message);
            return false;
        }
    }

    public async Task<bool> UpdateAnimeChannelAsync(ulong guildId, ulong animeChannelId)
    {
        try
        {
            var guild = await GetGuildAsync(guildId);
            if (guild is null)
            {
                await _dingleDbContext.Guilds.AddAsync(new Guild
                {
                    GuildId = guildId,
                    AnimeRemindersChannelId = animeChannelId
                });
            }
            else
            {
                guild.AnimeRemindersChannelId = animeChannelId;
                _dingleDbContext.Update(guild);
            }

            var rows = await _dingleDbContext.SaveChangesAsync();
            return rows != 0;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when setting anime channel id: {Message}", e.Message);
            return false;
        }
    }

    public async Task<Guild> GetGuildAsync(ulong guildId)
    {
        try
        {
            var guild = await _distributedCache.GetRecordAsync<Guild>(guildId.ToString());
            if (guild is not null) return guild;
            guild = await _dingleDbContext.Guilds.Where(x => x.GuildId == guildId).FirstOrDefaultAsync();
            if (guild is not null) await CacheModelAsync(guildId, guild, TimeSpan.FromMinutes(10));
            return guild;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when getting guild: {Message}", e.Message);
            return null;
        }
    }

    public async Task<User> GetUserAsync(ulong discordId)
    {
        try
        {
            var user = await _distributedCache.GetRecordAsync<User>(discordId.ToString());
            if (user is not null) return user;
            user = await _dingleDbContext.Users.Where(x => x.DiscordId == discordId).FirstOrDefaultAsync();
            await CacheModelAsync(discordId, user, TimeSpan.FromMinutes(10));
            return user;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when getting user: {Message}", e.Message);
            return null;
        }
    }

    public async Task<int> AddCoinsAsync(ulong discordId,
        int coins = 0, int fromInclusive = 100, int toExclusive = 201)
    {
        try
        {
            var amount = coins == 0 ? RandomNumberGenerator.GetInt32(fromInclusive, toExclusive) : coins;
            var user = await GetUserAsync(discordId);
            if (user is null)
            {
                await _dingleDbContext.Users.AddAsync(new User
                {
                    DiscordId = discordId,
                    Coins = 0
                });
            }
            else
            {
                user.Coins += amount;
                await CacheModelAsync(discordId, user, TimeSpan.FromMinutes(10));
                _dingleDbContext.Update(user);
            }

            var rows = await _dingleDbContext.SaveChangesAsync();
            return rows != 0 ? amount : 0;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when giving coins to user: {Message}", e.Message);
            return 0;
        }
    }

    public async Task<HashSet<Guild>> GetAllGuildsAsync()
    {
        try
        {
            var guilds = await _distributedCache.GetRecordAsync<HashSet<Guild>>("ALL_GUILDS");
            if (guilds is not null) return guilds;
            guilds = _dingleDbContext.Guilds.ToHashSet();
            await CacheModelAsync("ALL_GUILDS", guilds, TimeSpan.FromMinutes(10));
            return guilds;
        }
        catch (Exception e)
        {
            _logger.LogCritical("Exception when getting all guilds: {Message}", e.Message);
            return null;
        }
    }

    private async Task CacheModelAsync<TK, T>(TK key, T item, TimeSpan timeSpan)
    {
        await _distributedCache.SetRecordAsync(key.ToString(), item, timeSpan);
    }
}