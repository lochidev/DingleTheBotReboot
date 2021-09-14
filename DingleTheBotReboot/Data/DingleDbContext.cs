using DingleTheBotReboot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DingleTheBotReboot.Data
{
    public class DingleDbContext : DbContext
    {
        public DingleDbContext(DbContextOptions<DingleDbContext> options, IConfiguration config)
            : base(options)
        {
        }

        public DbSet<User> Users;
        public DbSet<Guild> Guilds;
    }
}