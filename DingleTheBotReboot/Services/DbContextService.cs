using System;
using System.Threading.Tasks;
using DingleTheBotReboot.Data;
using DingleTheBotReboot.Models;
using Microsoft.Extensions.Logging;

namespace DingleTheBotReboot.Services
{
    public class DbContextService : IDbContextService
    {
        private readonly DingleDbContext _dingleDbContext;
        private readonly ILogger<DbContextService> _logger;

        public DbContextService(DingleDbContext dingleDbContext, ILogger<DbContextService> logger)
        {
            _dingleDbContext = dingleDbContext;
            _logger = logger;
        }

        public async Task<bool> UpdateVerificationRoleAsync(ulong guildId, ulong verificationRoleId)
        {
            try
            {
                var guild = await _dingleDbContext.Guilds.FindAsync(guildId);
                if (guild is null)
                {
                    await _dingleDbContext.Guilds.AddAsync(new Guild()
                    {
                        GuildId = guildId,
                        VerificationRoleId = verificationRoleId
                    });
                }
                else
                {
                    guild.VerificationRoleId = verificationRoleId;
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
    }
}