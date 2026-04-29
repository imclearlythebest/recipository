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

    modelBuilder.Entity<OrderItem>(entity =>
    {
        entity.HasOne<Recipe>()
              .WithMany()
              .HasForeignKey(oi => oi.RecipeId)
              .OnDelete(DeleteBehavior.SetNull);
    });

    // --- MARKETPLACE & INGREDIENT SEED ---
    modelBuilder.Entity<ShopItem>().HasData(
      new ShopItem { Id = 1, Name = "Cherry Tomatoes", Description = "Fresh organic vine-ripened tomatoes", ImageUrl = "/images/tomato.jpg", Calories = 18, Price = 120.00f, Stock = 100, Moq = 1, Increment = 1, Type=IngredientType.Vegetable, UnitName = "per kg" },
      new ShopItem { Id = 2, Name = "Red Onion", Description = "Sharp and crunchy onions", ImageUrl = "/images/onion.jpg", Calories = 40, Price = 80.00f, Stock = 150, Moq = 1, Increment = 1, Type=IngredientType.Vegetable, UnitName = "per kg" },
      new ShopItem { Id = 3, Name = "Fresh Garlic", Description = "Aromatic garlic cloves", ImageUrl = "/images/garlic.jpg", Calories = 149, Price = 200.00f, Stock = 200, Moq = 1, Increment = 1, Type=IngredientType.Spice, UnitName = "per kg" },
      new ShopItem { Id = 4, Name = "Sea Salt", Description = "Fine-grain culinary sea salt", ImageUrl = "/images/salt.jpg", Calories = 0, Price = 40.00f, Stock = 300, Moq = 1, Increment = 1, Type=IngredientType.Spice, UnitName = "per kg" },
      new ShopItem { Id = 5, Name = "Extra Virgin Olive Oil", Description = "Cold-pressed Italian olive oil", ImageUrl = "/images/oil.jpg", Calories = 884, Price = 1250.00f, Stock = 50, Moq = 1, Increment = 1, Type=IngredientType.Other, UnitName = "per bottle" },
      new ShopItem { Id = 6, Name = "Chicken Breast", Description = "Skinless, boneless chicken breast", ImageUrl = "/images/chicken.jpg", Calories = 165, Price = 450.00f, Stock = 40, Moq = 1, Increment = 1, Type=IngredientType.Meat, UnitName = "per kg" },
      new ShopItem { Id = 8, Name = "Parmesan Cheese", Description = "Aged Italian hard cheese", ImageUrl = "/images/cheese.jpg", Calories = 431, Price = 1800.00f, Stock = 45, Moq = 1, Increment = 1, Type=IngredientType.Dairy, UnitName = "per kg" },
      new ShopItem { Id = 11, Name = "Premium Beef", Description = "Lean cuts of fresh beef", ImageUrl = "https://i.pinimg.com/1200x/8d/7d/7f/8d7d7f1c4ce9294370ffab6f5ef90e80.jpg", Calories = 250, Price = 750.00f, Stock = 30, Moq = 1, Increment = 1, Type=IngredientType.Meat, UnitName = "per kg" },
      new ShopItem { Id = 12, Name = "Fresh Ginger", Description = "Pungent and spicy ginger root", ImageUrl = "https://i.pinimg.com/1200x/67/06/a3/6706a3a460276d16afae19dcf3174700.jpg", Calories = 80, Price = 160.00f, Stock = 100, Moq = 1, Increment = 1, Type=IngredientType.Spice, UnitName = "per kg" }
    );

    string adminRoleId = "5307567e-613d-4c31-92f7-08064d142d7b";
    string adminUserId = "b74ddd14-6340-4840-95c2-db12554843e5";
    string chefMarioId = "a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0";
    string janeCooksId = "b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1";

    var hasher = new PasswordHasher<ApplicationUser>();

    modelBuilder.Entity<ApplicationUser>().HasData(
      new ApplicationUser
      {
        Id = adminUserId,
        UserName = "admin",
        NormalizedUserName = "ADMIN",
        Email = "admin@recipository.com",
        NormalizedEmail = "ADMIN@RECIPOSITORY.COM",
        EmailConfirmed = true,
        DisplayName = "System Admin",
        AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=200",
        SecurityStamp = Guid.NewGuid().ToString(),
        PasswordHash = hasher.HashPassword(null!, "Admin123!"),
        Bio = "Maintaining the platform and curating top recipes."
      },
      new ApplicationUser
      {
        Id = chefMarioId,
        UserName = "mario",
        NormalizedUserName = "MARIO",
        Email = "mario@chef.com",
        NormalizedEmail = "MARIO@CHEF.COM",
        EmailConfirmed = true,
        DisplayName = "Chef Mario",
        AvatarUrl = "https://i.pinimg.com/736x/55/f2/8f/55f28f19e26992ee6762131db21e9a90.jpg",
        SecurityStamp = Guid.NewGuid().ToString(),
        PasswordHash = hasher.HashPassword(null!, "Chef123!"),
        Bio = "Professional chef with 20 years of experience in Italian cuisine."
      },
      new ApplicationUser
      {
        Id = janeCooksId,
        UserName = "jane",
        NormalizedUserName = "JANE",
        Email = "jane@homecook.com",
        NormalizedEmail = "JANE@HOMECOOK.COM",
        EmailConfirmed = true,
        DisplayName = "Jane Cooks",
        AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200",
        SecurityStamp = Guid.NewGuid().ToString(),
        PasswordHash = hasher.HashPassword(null!, "Jane123!"),
        Bio = "Passionate home cook who loves quick and healthy meals."
      }
    );

    modelBuilder.Entity<Recipe>().HasData(
      new Recipe { 
          Id = 1, 
          Title = "Creamy Tuscan Chicken", 
          CoverImageUrl = "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=800",
          MainText = "A rich and flavorful Italian classic that comes together in under 30 minutes.", 
          Instructions = "1. Season chicken with salt and pepper.\n2. Pan-sear chicken until golden.\n3. Add garlic and parmesan to the pan.\n4. Stir in tomatoes and spinach until wilted.\n5. Serve over your favorite side.", 
          CreatedAt = new DateTime(2024, 6, 1), 
          ApplicationUserId = chefMarioId, 
          PrepTime = 10, 
          CookTime = 20, 
          ServingSize = 4, 
          Status = "Published",
          PublishedAt = new DateTime(2024, 6, 2)
      },
      new Recipe { 
          Id = 3, 
          Title = "Healthy Garden Salad", 
          CoverImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800",
          MainText = "A refreshing and nutrient-packed salad perfect for a light lunch.", 
          Instructions = "1. Chop tomatoes and onions.\n2. Combine with fresh greens.\n3. Drizzle with extra virgin olive oil.\n4. Season with sea salt and cracked pepper.", 
          CreatedAt = new DateTime(2024, 6, 3), 
          ApplicationUserId = janeCooksId, 
          PrepTime = 10, 
          CookTime = 0, 
          ServingSize = 1, 
          Status = "Published",
          PublishedAt = new DateTime(2024, 6, 4)
      },
      new Recipe { 
          Id = 4, 
          Title = "Spicy Beef Bhuna", 
          CoverImageUrl = "https://i.pinimg.com/1200x/d1/fd/bf/d1fdbf4f05852965cb6168d8a1804d6f.jpg",
          MainText = "A traditional Bangladeshi beef curry, slow-cooked to perfection with aromatic spices.", 
          Instructions = "1. Sauté onions and garlic in oil until golden.\n2. Add beef cubes and ginger paste.\n3. Stir in salt, turmeric, and chili powder.\n4. Cook on low heat until the beef is tender and the oil separates.\n5. Serve hot with steamed rice or paratha.", 
          CreatedAt = new DateTime(2024, 6, 4), 
          ApplicationUserId = janeCooksId, 
          PrepTime = 20, 
          CookTime = 60, 
          ServingSize = 4, 
          Status = "Published",
          PublishedAt = new DateTime(2024, 6, 5)
      }
    );

    modelBuilder.Entity<RecipeIngredient>().HasData(
        new RecipeIngredient { Id = 1, RecipeId = 1, IngredientId = 6, Quantity = 500, Unit = "g", CaloriesPerUnit = 1.65f },
        new RecipeIngredient { Id = 3, RecipeId = 1, IngredientId = 8, Quantity = 50, Unit = "g", CaloriesPerUnit = 4.31f },
        new RecipeIngredient { Id = 6, RecipeId = 3, IngredientId = 1, Quantity = 150, Unit = "g", CaloriesPerUnit = 0.18f },
        new RecipeIngredient { Id = 7, RecipeId = 3, IngredientId = 5, Quantity = 15, Unit = "ml", CaloriesPerUnit = 8.84f },
        new RecipeIngredient { Id = 8, RecipeId = 4, IngredientId = 11, Quantity = 800, Unit = "g", CaloriesPerUnit = 2.50f },
        new RecipeIngredient { Id = 9, RecipeId = 4, IngredientId = 2, Quantity = 200, Unit = "g", CaloriesPerUnit = 0.40f },
        new RecipeIngredient { Id = 10, RecipeId = 4, IngredientId = 12, Quantity = 30, Unit = "g", CaloriesPerUnit = 0.80f }
    );

    modelBuilder.Entity<RecipeReview>().HasData(
        new RecipeReview { Id = 1, RecipeId = 1, ApplicationUserId = janeCooksId, Rating = 5, ReviewText = "Amazing! My family loved it. So creamy.", CreatedAt = DateTime.UtcNow.AddDays(-5) }
    );

    modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
    {
      Id = adminRoleId,
      Name = "Admin",
      NormalizedName = "ADMIN",
      ConcurrencyStamp = adminRoleId
    });

    modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
    {
      RoleId = adminRoleId,
      UserId = adminUserId
    });
  }
}
