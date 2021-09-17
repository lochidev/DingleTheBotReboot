﻿using System.Threading.Tasks;
using DingleTheBotReboot.Models;

namespace DingleTheBotReboot.Services
{
    public interface IDbContextService
    {
        public Task<bool> UpdateVerificationRoleAsync(ulong guildId, ulong verificationRoleId);
        public Task<Guild> GetGuildAsync(ulong guildId);
        public Task<User> GetUserAsync(ulong discordId);

        /// <summary>
        /// Adds a given amount of coins, random amount by default, to the specified user.
        /// </summary>
        /// <returns>
        /// The coins given, 0 if failed
        /// </returns>
        public Task<int> AddCoinsAsync(ulong discordId,
            int coins = 0, int fromInclusive = 100, int toExclusive = 201);
    }
}