namespace Website.Models.Dtos;

public class RevenueTransactionDto
{
    public int Id { get; set; }
    public string RecipeTitle { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal AuthorShare { get; set; }
    public string Status { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
}