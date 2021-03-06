using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;

namespace DingleTheBotReboot.Data;

public class DingleDbContext : DbContext
{
    public DingleDbContext(DbContextOptions<DingleDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<Item> Items { get; set; }
}