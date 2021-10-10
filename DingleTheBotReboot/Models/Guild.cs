using System.ComponentModel.DataAnnotations;

namespace DingleTheBotReboot.Models;

public class Guild
{
    [Key] public ulong GuildId { get; set; }

    public ulong VerificationRoleId { get; set; }
    public ulong AnimeRemindersChannelId { get; set; }
}