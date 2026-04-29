namespace Website.Models.Dtos;

public class RecipeResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string MainText { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookTime { get; set; }
    public int ServingSize { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public float EstimatedCalories { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = [];
}

public class RecipeIngredientDto
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public float Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public float TotalCalories { get; set; }
}
