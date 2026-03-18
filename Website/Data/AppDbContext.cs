using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Content> Contents => Set<Content>();

    // Seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
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
    }
}