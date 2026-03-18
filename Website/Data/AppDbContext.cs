using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  // Satisfies the IngredientsController
  public DbSet<ShopItem> Ingredients { get; set; }

  // Satisfies the HomeController
  public DbSet<ShopItem> Contents { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Seed data so the marketplace has items with real IDs (1 and 2)
    modelBuilder.Entity<ShopItem>().HasData(
      new ShopItem { 
        Id = 1, 
        Name = "Salt", 
        Description = "Fine sea salt for seasoning.", 
        Price = 1.50f, 
        ImageUrl = "/images/salt.jpg" 
      },
      new ShopItem { 
        Id = 2, 
        Name = "Olive Oil", 
        Description = "Extra virgin olive oil.", 
        Price = 8.99f, 
        ImageUrl = "/images/oil.jpg" 
      }
    );
  }
}