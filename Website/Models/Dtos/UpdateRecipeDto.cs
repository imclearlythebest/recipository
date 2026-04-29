using System.ComponentModel.DataAnnotations;

namespace Website.Models.Dtos;

public class UpdateRecipeDto
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Recipe title is required")]
    [StringLength(200, MinimumLength = 5, 
        ErrorMessage = "Title must be between 5 and 200 characters")]
    public string Title { get; set; } = null!;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string MainText { get; set; } = null!;
    
    [Required(ErrorMessage = "Instructions are required")]
    [StringLength(5000, MinimumLength = 20,
        ErrorMessage = "Instructions must be between 20 and 5000 characters")]
    public string Instructions { get; set; } = null!;
    
    [Range(0, 480, ErrorMessage = "Prep time must be between 0 and 480 minutes")]
    public int PrepTime { get; set; }
    
    [Range(0, 480, ErrorMessage = "Cook time must be between 0 and 480 minutes")]
    public int CookTime { get; set; }
    
    [Range(1, 100, ErrorMessage = "Serving size must be between 1 and 100")]
    public int ServingSize { get; set; }
    
    [Required]
    [RegularExpression("Easy|Medium|Hard", 
        ErrorMessage = "Difficulty must be Easy, Medium, or Hard")]
    public string Difficulty { get; set; } = "Medium";
    
    [Required(ErrorMessage = "At least one ingredient is required")]
    [MinLength(1, ErrorMessage = "At least one ingredient is required")]
    public List<CreateRecipeIngredientDto> Ingredients { get; set; } = [];
}