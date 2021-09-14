using System.Threading.Tasks;

namespace DingleTheBotReboot.Services
{
    public interface IDbContextService
    {
        public Task<bool> UpdateVerificationRoleAsync(ulong guildId, ulong verificationRoleId); 
    }
}