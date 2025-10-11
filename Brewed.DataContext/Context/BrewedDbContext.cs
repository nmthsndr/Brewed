using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Entities;

namespace Brewed.DataContext.Context
{
    public class BrewedDbContext : DbContext
    {
        public BrewedDbContext(DbContextOptions<BrewedDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Egyedi email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //Egy kategóriában több termék lehet
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            //Ár pontosság
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            //Összeg pontosság a rendelésnél
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);
        }
    }
}
