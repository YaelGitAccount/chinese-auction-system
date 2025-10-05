using Microsoft.EntityFrameworkCore;
using server_NET.Models;

namespace server_NET.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<LotteryResult> LotteryResults { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LotteryResult relationship
            modelBuilder.Entity<LotteryResult>()
                .HasOne(lr => lr.WinnerUser)
                .WithMany()
                .HasForeignKey(lr => lr.WinnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Gift-Category relationship
            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Gifts)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Price column with precision
            modelBuilder.Entity<Gift>()
                .Property(g => g.Price)
                .HasPrecision(18, 2);

            // הגדרת אינדקס ייחודי על אימייל של תורם
            modelBuilder.Entity<Donor>()
                .HasIndex(d => d.Email)
                .IsUnique();
        }
    }
}
