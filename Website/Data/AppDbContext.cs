using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
}