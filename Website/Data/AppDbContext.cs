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
  public DbSet<Recipe> Recipes => Set<Recipe>();
  public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
  public DbSet<RecipeApproval> RecipeApprovals => Set<RecipeApproval>();
  public DbSet<RecipeRevenue> RecipeRevenues => Set<RecipeRevenue>();
  public DbSet<ContentVote> ContentVotes => Set<ContentVote>();
  public DbSet<Follow> Follows => Set<Follow>();
  public DbSet<Collection> Collections => Set<Collection>();
  public DbSet<RecipeReviewRequest> RecipeReviewRequests => Set<RecipeReviewRequest>();
  public DbSet<RecipeReview> RecipeReviews => Set<RecipeReview>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<CartItem> CartItems => Set<CartItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<CartItem>(entity =>
    {
      entity.HasOne(ci => ci.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(ci => ci.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ShopItemId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Ingredient>()
      .HasDiscriminator<string>("IngredientType")
      .HasValue<Ingredient>("Base")
      .HasValue<ShopItem>("Shop");
    
    modelBuilder.Entity<Content>(entity =>
    {
      entity.HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(c => c.Author)
            .WithMany(u => u.Recipes)
            .HasForeignKey(c => c.ApplicationUserId)
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

    modelBuilder.Entity<Follow>(entity =>
    {
      entity.HasIndex(f => new { f.FollowerId, f.FollowedId })
            .IsUnique();

      entity.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);

      entity.HasOne(f => f.Followed)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowedId)
            .OnDelete(DeleteBehavior.NoAction);

    });

    modelBuilder.Entity<Collection>(entity =>
    {
      entity.HasIndex(c => c.PublicUrl)
        .IsUnique();

      entity.HasOne(c => c.User)
            .WithMany(u => u.Collections)
            .HasForeignKey(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasMany(c => c.Recipes)
            .WithMany(r => r.Collections)
            .UsingEntity(j => j.ToTable("CollectionRecipes"));
    });

        modelBuilder.Entity<RecipeReviewRequest>(entity =>
        {
      entity.HasIndex(r => new { r.ApplicationUserId, r.RecipeId })
        .IsUnique();

      entity.HasOne(r => r.User)
        .WithMany(u => u.ReviewRequests)
        .HasForeignKey(r => r.ApplicationUserId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(r => r.Recipe)
        .WithMany(r => r.ReviewRequests)
        .HasForeignKey(r => r.RecipeId)
        .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeReview>(entity =>
        {
      entity.HasIndex(r => new { r.ApplicationUserId, r.RecipeId })
        .IsUnique();

      entity.Property(r => r.Rating)
        .HasDefaultValue(5);

      entity.HasOne(r => r.User)
        .WithMany(u => u.Reviews)
        .HasForeignKey(r => r.ApplicationUserId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(r => r.Recipe)
        .WithMany(r => r.Reviews)
        .HasForeignKey(r => r.RecipeId)
        .OnDelete(DeleteBehavior.Cascade);
        });

    modelBuilder.Entity<Recipe>(entity =>
    {
      entity.HasMany(r => r.Ingredients)
            .WithMany()
            .UsingEntity<RecipeIngredient>(
                join => join
                    .HasOne(ri => ri.Ingredient)
                    .WithMany()
                    .HasForeignKey(ri => ri.IngredientId)
                    .OnDelete(DeleteBehavior.Restrict),
                join => join
                    .HasOne(ri => ri.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey(ri => ri.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey(ri => ri.Id);
                    join.ToTable("RecipeIngredients");
                });
    });

    modelBuilder.Entity<RecipeIngredient>(entity =>
    {
      entity.HasKey(ri => ri.Id);
      entity.HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(ri => ri.Ingredient)
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<RecipeApproval>(entity =>
    {
      entity.HasKey(ra => ra.Id);
      entity.HasOne(ra => ra.Recipe)
            .WithMany(r => r.ApprovalHistory)
            .HasForeignKey(ra => ra.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(ra => ra.ApprovedBy)
            .WithMany()
            .HasForeignKey(ra => ra.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<RecipeRevenue>(entity =>
    {
      entity.HasKey(rr => rr.Id);
      entity.HasOne(rr => rr.Recipe)
            .WithMany(r => r.Revenues)
            .HasForeignKey(rr => rr.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
      entity.HasOne(rr => rr.OrderItem)
            .WithMany()
            .HasForeignKey(rr => rr.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
      entity.Property(rr => rr.Amount).HasPrecision(18, 2);
      entity.Property(rr => rr.AuthorShare).HasPrecision(18, 2);
      entity.Property(rr => rr.PlatformShare).HasPrecision(18, 2);
    });

    // --- MARKETPLACE & INGREDIENT SEED ---
    modelBuilder.Entity<ShopItem>().HasData(
      new ShopItem { Id = 1, Name = "Tomato", Description = "Red, juicy fruit", ImageUrl = "/images/tomato.jpg", Calories = 20, Price = 0.00f, Stock = 100, Moq = 1, Increment = 1, Type=IngredientType.Vegetable },
      new ShopItem { Id = 2, Name = "Onion", Description = "Pungent bulb", ImageUrl = "/images/onion.jpg", Calories = 40, Price = 0.30f, Stock = 50, Moq = 1, Increment = 1, Type=IngredientType.Vegetable },
      new ShopItem { Id = 3, Name = "Garlic", Description = "Strong-flavored bulb", ImageUrl = "/images/garlic.jpg", Calories = 30, Price = 0.20f, Stock = 100, Moq = 1, Increment = 1, Type=IngredientType.Spice },
      new ShopItem { Id = 4, Name = "Salt", Description = "Fine sea salt", ImageUrl = "/images/salt.jpg", Calories = 0, Price = 1.50f, Stock = 0, Moq = 1, Increment = 1, Type=IngredientType.Spice },
      new ShopItem { Id = 5, Name = "Olive Oil", Description = "Extra virgin", ImageUrl = "/images/oil.jpg", Calories = 120, Price = 8.99f, Stock = 50, Moq = 1, Increment = 1, Type=IngredientType.Other }
    );

    string adminRoleId = "5307567e-613d-4c31-92f7-08064d142d7b"; // Fixed GUID
    string adminUserId = "b74ddd14-6340-4840-95c2-db12554843e5"; // Fixed GUID

    modelBuilder.Entity<Recipe>().HasData(
      new Recipe { Id = 1, Title = "How To Make Souvlaki", MainText = "This is some content.", CreatedAt = new DateTime(2024, 6, 1), ApplicationUserId = adminUserId },
      new Recipe { Id = 2, Title = "Bongoposhagor-ian Dish", MainText = "This is some more content.", CreatedAt = new DateTime(2024, 6, 2), ApplicationUserId = adminUserId },
      new Recipe { Id = 3, Title = "How To Make Bhelpuri", MainText = "This is even more content.", CreatedAt = new DateTime(2024, 6, 3), ApplicationUserId = adminUserId }
    );

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
      PasswordHash = hasher.HashPassword(null!, "Admin123!"),
      Bio = "I am the admin."
    });

    modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
    {
      RoleId = adminRoleId,
      UserId = adminUserId
    });
  }
}
