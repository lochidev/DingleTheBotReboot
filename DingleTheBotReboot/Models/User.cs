using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DingleTheBotReboot.Models
{
    public class User
    {
        [Key] public ulong DiscordId { get; set; }

        public int Coins { get; set; }
        public List<Item> Items { get; set; }
    }
}