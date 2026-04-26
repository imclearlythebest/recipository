using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<Ingredient> Ingredients => Set<Ingredient>();
  public DbSet<ShopItem> ShopItems => Set<ShopItem>();
  public DbSet<Content> Contents => Set<Content>();
  public DbSet<ContentVote> ContentVotes => Set<ContentVote>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Ingredient>()
      .HasDiscriminator<string>("IngredientType")
      .HasValue<Ingredient>("Base")
      .HasValue<ShopItem>("Shop");
    
    modelBuilder.Entity<Content>(entity =>
    {
      entity.HasOne(c => c.Parent)
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
            
    });

    modelBuilder.Entity<ContentVote>(entity =>
    {
      entity.HasIndex(v => new { v.ApplicationUserId, v.ContentId })
            .IsUnique();

      entity.HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .HasForeignKey(v => v.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(v => v.Content)
            .WithMany(c => c.Votes)
            .HasForeignKey(v => v.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.Property(v => v.VoteType)
            .HasConversion<int>();

    });


    // --- MARKETPLACE & INGREDIENT SEED ---
    modelBuilder.Entity<ShopItem>().HasData(
      new ShopItem { Id = 1, Name = "Tomato", Description = "Red, juicy fruit", ImageUrl = "/images/tomato.jpg", Calories = 20, Price = 0.50f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 2, Name = "Onion", Description = "Pungent bulb", ImageUrl = "/images/onion.jpg", Calories = 40, Price = 0.30f, Stock = 50, Moq = 1, Increment = 1 },
      new ShopItem { Id = 3, Name = "Garlic", Description = "Strong-flavored bulb", ImageUrl = "/images/garlic.jpg", Calories = 30, Price = 0.20f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 4, Name = "Salt", Description = "Fine sea salt", ImageUrl = "/images/salt.jpg", Calories = 0, Price = 1.50f, Stock = 100, Moq = 1, Increment = 1 },
      new ShopItem { Id = 5, Name = "Olive Oil", Description = "Extra virgin", ImageUrl = "/images/oil.jpg", Calories = 120, Price = 8.99f, Stock = 50, Moq = 1, Increment = 1 }
    );

    modelBuilder.Entity<Content>().HasData(
      new Content { Id = 1, MainText = "This is some content.", CreatedAt = new DateTime(2024, 6, 1) },
      new Content { Id = 2, MainText = "This is some more content.", CreatedAt = new DateTime(2024, 6, 2) },
      new Content { Id = 3, MainText = "This is even more content.", CreatedAt = new DateTime(2024, 6, 3) }
    );

    // --- ADMIN SEEDING (FIXED) ---
    string adminRoleId = "5307567e-613d-4c31-92f7-08064d142d7b"; // Fixed GUID
    string adminUserId = "b74ddd14-6340-4840-95c2-db12554843e5"; // Fixed GUID

    modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
    {
      Id = adminRoleId,
      Name = "Admin",
      NormalizedName = "ADMIN",
      ConcurrencyStamp = adminRoleId
    });

    var hasher = new PasswordHasher<ApplicationUser>();
    modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
    {
      Id = adminUserId,
      UserName = "admin",
      NormalizedUserName = "ADMIN",
      Email = "admin@recipository.com",
      NormalizedEmail = "ADMIN@RECIPOSITORY.COM",
      EmailConfirmed = true,
      DisplayName = "System Admin",
      SecurityStamp = "786720e7-3f8d-4252-944d-4566f7f6f592", // Required for Login
      PasswordHash = hasher.HashPassword(null!, "Admin123!")
    });

    modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
    {
      RoleId = adminRoleId,
      UserId = adminUserId
    });
  }
}
