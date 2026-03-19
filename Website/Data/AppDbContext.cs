using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<Ingredient> Ingredients => Set<Ingredient>();
  public DbSet<Content> Contents => Set<Content>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Seed everything as ShopItems so they have Price/Stock for the Marketplace
    modelBuilder.Entity<ShopItem>().HasData(
      new ShopItem { Id = 1, Name = "Tomato", Description = "Red, juicy fruit", ImageUrl = "/images/tomato.jpg", Calories = 20, Price = 0.50f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 2, Name = "Onion", Description = "Pungent bulb", ImageUrl = "/images/onion.jpg", Calories = 40, Price = 0.30f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 3, Name = "Garlic", Description = "Strong-flavored bulb", ImageUrl = "/images/garlic.jpg", Calories = 30, Price = 0.20f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 4, Name = "Salt", Description = "Fine sea salt", ImageUrl = "/images/salt.jpg", Calories = 0, Price = 1.50f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 5, Name = "Olive Oil", Description = "Extra virgin", ImageUrl = "/images/oil.jpg", Calories = 120, Price = 8.99f, Stock = 50, Moq = 1, Increment = 1 }
    );

    modelBuilder.Entity<Content>().HasData(
      new Content { Id = 1, MainText = "This is some content.", CreatedAt = new DateTime(2024, 6, 1) },
      new Content { Id = 2, MainText = "This is some more content.", CreatedAt = new DateTime(2024, 6, 2) },
      new Content { Id = 3, MainText = "This is even more content.", CreatedAt = new DateTime(2024, 6, 3) }
    );
  }
}