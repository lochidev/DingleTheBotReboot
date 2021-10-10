using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("Dingle");
        var ulongConverter = new NumberToStringConverter<ulong>();
        modelBuilder.Entity<User>()
            .Property(o => o.DiscordId)
            .HasConversion(ulongConverter);
        modelBuilder.Entity<User>()
            .ToContainer("Users")
            .HasNoDiscriminator()
            .HasPartitionKey(o => o.DiscordId);
        modelBuilder.Entity<Guild>()
            .Property(o => o.GuildId)
            .HasConversion(ulongConverter);
        modelBuilder.Entity<Guild>()
            .ToContainer("Guilds")
            .HasNoDiscriminator()
            .HasPartitionKey(o => o.GuildId);
        var intConverter = new NumberToStringConverter<int>();
        modelBuilder.Entity<Item>()
            .Property(o => o.Id)
            .HasConversion(intConverter);
        modelBuilder.Entity<Item>()
            .ToContainer("Items")
            .HasNoDiscriminator()
            .HasPartitionKey(o => o.Id);
    }
}