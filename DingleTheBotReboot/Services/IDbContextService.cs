using System.Threading.Tasks;
using DingleTheBotReboot.Models;

namespace DingleTheBotReboot.Services
{
    public interface IDbContextService
    {
        public Task<bool> UpdateVerificationRoleAsync(ulong guildId, ulong verificationRoleId);
        public Task<Guild> GetGuildAsync(ulong guildId);
    }
}