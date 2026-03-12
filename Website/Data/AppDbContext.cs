using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    

    // Seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<Ingredient>().HasData(
        new Ingredient { Id = 1, Name = "Tomato", Description = "Red, juicy fruit", ImageUrl = "/images/tomato.jpg", Calories = 20 },
        new Ingredient { Id = 2, Name = "Onion", Description = "Pungent bulb", ImageUrl = "/images/onion.jpg", Calories = 40 },
        new Ingredient { Id = 3, Name = "Garlic", Description = "Strong-flavored bulb", ImageUrl = "/images/garlic.jpg", Calories = 30 }
    );
    }
}