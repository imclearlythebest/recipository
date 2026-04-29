namespace Website.Models;

public class RecipeRevenue
{
    public int Id { get; set; }
    
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public decimal AuthorShare { get; set; }
    public decimal PlatformShare { get; set; }
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    
    public DateTime? CompletedDate { get; set; }
    public DateTime? RefundedDate { get; set; }
    public string? RefundReason { get; set; }
}
