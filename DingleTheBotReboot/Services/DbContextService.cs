using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DingleTheBotReboot.Services
{
    public class DbContextService : IDbContextService
    {
        private readonly DingleDbContext _dingleDbContext;
        private readonly ILogger<DbContextService> _logger;
        private readonly IMemoryCache _memoryCache;

        public DbContextService(DingleDbContext dingleDbContext, ILogger<DbContextService> logger, Random rnd,
            IMemoryCache memoryCache)
        {
            _dingleDbContext = dingleDbContext;
            _logger = logger;
            _memoryCache = memoryCache;
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

        public async Task<Guild> GetGuildAsync(ulong guildId)
        {
            try
            {
                if (!_memoryCache.TryGetValue(guildId, out Guild guild))
                {
                    guild = await _dingleDbContext.Guilds.Where(x => x.GuildId == guildId).FirstOrDefaultAsync();
                    await CacheModel(guildId, guild, TimeSpan.FromMinutes(10));
                }
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
                if (!_memoryCache.TryGetValue(discordId, out User user))
                {
                    user = await _dingleDbContext.Users.Where(x => x.DiscordId == discordId).FirstOrDefaultAsync();
                    await CacheModel(discordId, user, TimeSpan.FromMinutes(10));
                }
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
                    await CacheModel(discordId, user, TimeSpan.FromMinutes(10));
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
        private Task<T> CacheModel<T>(ulong key, T item, TimeSpan timeSpan)
        {
            return item is not null ? Task.FromResult(_memoryCache.Set(key, item, timeSpan)) : null;
        }
    }
}