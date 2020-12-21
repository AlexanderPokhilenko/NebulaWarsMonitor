using Microsoft.EntityFrameworkCore;
using NebulaWarsMonitor.Models.EF;

namespace NebulaWarsMonitor
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }
        public DbSet<LogRecord> LogRecords { get; set; }

        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Server>().HasIndex(p => p.Address).IsUnique();
            modelBuilder.Entity<MessageType>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<MessageType>().HasData(
                new MessageType(MessageTypeEnum.Info),
                new MessageType(MessageTypeEnum.Warning),
                new MessageType(MessageTypeEnum.Error),
                new MessageType(MessageTypeEnum.Fatal)
                );
        }
    }
}
