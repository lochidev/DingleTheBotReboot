using System.ComponentModel.DataAnnotations;

namespace DingleTheBotReboot.Models
{
    public class Item
    {
        [Key] public int Id { get; set; }

        public string Name { get; set; }
        public int Price { get; set; }
    }
}