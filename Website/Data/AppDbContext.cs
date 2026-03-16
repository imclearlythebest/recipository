using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Collection> Collections => Set<Collection>();

    // Seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<Ingredient>().HasData(
        new Ingredient { Id = 1, Name = "Tomato", Description = "Red, juicy fruit", ImageUrl = "/images/tomato.jpg", Calories = 20 },
        new Ingredient { Id = 2, Name = "Onion", Description = "Pungent bulb", ImageUrl = "/images/onion.jpg", Calories = 40 },
        new Ingredient { Id = 3, Name = "Garlic", Description = "Strong-flavored bulb", ImageUrl = "/images/garlic.jpg", Calories = 30 }
    );
    modelBuilder.Entity<Content>().HasData(
        new Content { Id = 1, MainText = "This is some content.", CreatedAt = new DateTime(2024, 6, 1) },
        new Content { Id = 2, MainText = "This is some more content.", CreatedAt = new DateTime(2024, 6, 2) },
        new Content { Id = 3, MainText = "This is even more content.", CreatedAt = new DateTime(2024, 6, 3) }
    );
    modelBuilder.Entity<Collection>().HasData(
        new Collection { Id = 1, Name = "My Collection", Description = "A collection of ingredients", ImageUrl = "/images/tomato.jpg" },
        new Collection { Id = 2, Name = "Another Collection", Description = "Another collection of ingredients", ImageUrl = "/images/onion.jpg" },
        new Collection { Id = 3, Name = "Yet Another Collection", Description = "Yet another collection of ingredients", ImageUrl = "/images/garlic.jpg" }
    );
    }
}