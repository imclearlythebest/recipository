namespace Website.Models;

public class Order
{
  public int Id { get; set; }
  public string UserId { get; set; } = string.Empty;
  public DateTime OrderDate { get; set; } = DateTime.UtcNow;
  public float TotalAmount { get; set; }
  
  // Default status for new orders
  public string Status { get; set; } = "Processing"; 

  public List<OrderItem> OrderItems { get; set; } = new();
  
  // Navigation property for Identity
  public ApplicationUser? User { get; set; }
}