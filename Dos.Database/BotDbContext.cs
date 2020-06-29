using System;
using System.Configuration;
using Dos.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Dos.Database
{
    public class BotDbContext : DbContext
    {
        public DbSet<GuildConfig> GuildConfig { get; set; }

        public BotDbContext()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GuildConfig>(b =>
            {
                b.Property(c => c.GuildConfigId).ValueGeneratedOnAdd();
                b.HasIndex(c => c.GuildId).IsUnique();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            var connString = Environment.GetEnvironmentVariable("ConnectionString");

            if (!string.IsNullOrWhiteSpace(connString))
                return connString;

            throw new ArgumentException("Failed to find connections string");
        } 
    }
}
