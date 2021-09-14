using System.Collections.Generic;

namespace DingleTheBotReboot.Models
{
    public class User
    {
        public long DiscordId { get; set; }
        public int Coins { get; set; }
        public List<IItem> Items { get; set; }
    }
}