namespace Website.Models.Dtos;

public class PendingRecipeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ApprovalHistoryDto
{
    public int Id { get; set; }
    public string AdminName { get; set; } = null!;
    public DateTime ReviewedAt { get; set; }
    public string Status { get; set; } = null!;
    public string? Reason { get; set; }
}