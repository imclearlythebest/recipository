using System.ComponentModel.DataAnnotations.Schema;

namespace Website.Models;

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    
    public float Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public float CaloriesPerUnit { get; set; }
    
    [NotMapped]
    public float TotalCalories => Quantity * CaloriesPerUnit;
}
