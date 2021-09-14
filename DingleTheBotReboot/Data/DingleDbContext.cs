using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DingleTheBotReboot.Data
{
    public class DingleDbContext : DbContext
    {
        public DingleDbContext (DbContextOptions<DingleDbContext> options, IConfiguration config)
            : base(options)
        {
            
        }
    }
}