using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DingleTheBotReboot.Services
{
    public class DbContextService : IDbContextService
    {
        private readonly DingleDbContext _dingleDbContext;
        private readonly ILogger<DbContextService> _logger;

        public DbContextService(DingleDbContext dingleDbContext, ILogger<DbContextService> logger, Random rnd)
        {
            _dingleDbContext = dingleDbContext;
            _logger = logger;
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
                _logger.LogCritical("Exception when creating guild: {Message}", e.Message);
                return false;
            }
        }

        public async Task<Guild> GetGuildAsync(ulong guildId)
        {
            return await _dingleDbContext.Guilds.Where(x => x.GuildId == guildId).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserAsync(ulong discordId)
        {
            return await _dingleDbContext.Users.Where(x => x.DiscordId == discordId).FirstOrDefaultAsync();
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
    }
}