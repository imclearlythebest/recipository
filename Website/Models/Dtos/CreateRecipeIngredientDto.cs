using System.ComponentModel.DataAnnotations;

namespace Website.Models.Dtos;

public class CreateRecipeIngredientDto
{
    [Required]
    public int IngredientId { get; set; }
    
    [Range(0.1, 10000, ErrorMessage = "Quantity must be between 0.1 and 10000")]
    public float Quantity { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;
}
