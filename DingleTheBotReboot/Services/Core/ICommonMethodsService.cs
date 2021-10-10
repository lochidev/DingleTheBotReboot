using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace DingleTheBotReboot.Services;

/// <summary>
///     An interface for common code shared by responders and commands
/// </summary>
public interface ICommonMethodsService
{
    public Task<ulong> NukeChannelAsync(IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        Snowflake channelId,
        Snowflake guildId,
        IUser user);
}