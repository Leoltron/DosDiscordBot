using System;
using Dos.Database.Models;
using Dos.Game;
using Dos.Game.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;

namespace Dos.Database
{
    public class DosDbContext : DbContext
    {
        static DosDbContext()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CardJsonConverter());
            settings.Converters.Add(new NullableCardJsonConverter());
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet(settings:settings);
        }
        
        public DbSet<GuildConfig> GuildConfig { get; set; }
        public DbSet<Replay> Replay { get; set; }
        public DbSet<ReplayMove> ReplayMove { get; set; }
        public DbSet<ReplayPlayer> ReplayPlayer { get; set; }
        public DbSet<ReplaySnapshot> ReplaySnapshot { get; set; }

        public DosDbContext()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Database.EnsureCreated();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<GameLogEventType>();
        }

        public DosDbContext(DbContextOptions<DosDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresEnum<GameLogEventType>();
            
            builder.Entity<GuildConfig>(b =>
            {
                b.Property(c => c.GuildConfigId).ValueGeneratedOnAdd();
                b.HasIndex(c => c.GuildId).IsUnique();
            });

            builder.Entity<Replay>(b =>
            {
                b.Property(e => e.ReplayId).ValueGeneratedOnAdd();
                b.HasIndex(e => e.GameStartDate);
            });

            builder.Entity<ReplayMove>(b =>
            {
                b.HasOne(rm => rm.Replay).WithMany(r => r.Moves).HasForeignKey(rm => rm.ReplayId);
            });

            builder.Entity<ReplayPlayer>(b =>
            {
                b.HasOne(rp => rp.Replay).WithMany(r => r.Players).HasForeignKey(rp => rp.ReplayId);
            });

            builder.Entity<ReplaySnapshot>(b =>
            {
                b.HasOne(rs => rs.Replay).WithMany(r => r.Snapshots).HasForeignKey(rs => rs.ReplayId);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            var connString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            if (!string.IsNullOrWhiteSpace(connString))
                return connString;

            throw new ArgumentException("Failed to find connection string");
        } 
    }
}
