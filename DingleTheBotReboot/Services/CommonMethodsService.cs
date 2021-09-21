using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace DingleTheBotReboot.Services
{
    public class CommonMethodsService : ICommonMethodsService
    {
        public async Task<ulong> NukeChannelAsync(IDiscordRestGuildAPI guildApi,
            IDiscordRestChannelAPI channelApi,
            Snowflake channelId,
            Snowflake guildId,
            IUser user)
        {
            var getChannelReply = await channelApi.GetChannelAsync(channelId);
            if (!getChannelReply.IsSuccess) return 0;
            var channel = getChannelReply.Entity;
            var channelCreateReply = await guildApi.CreateGuildChannelAsync(
                guildId,
                channel.Name.Value,
                ChannelType.GuildText,
                channel.Topic,
                position: channel.Position,
                permissionOverwrites: channel.PermissionOverwrites,
                parentID: channel.ParentID.HasValue ? channel.ParentID.Value.Value : default,
                isNsfw: channel.IsNsfw,
                reason: "From nuke");
            await channelApi.DeleteChannelAsync(channelId,
                $"Nuked by {user.Username}#{user.Discriminator}");
            return channelCreateReply.IsSuccess ? channelCreateReply.Entity.ID.Value : 0;
        }
    }
}