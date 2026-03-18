namespace Website.Models;

public class Content
{
    public int Id { get; set; }
    public required string MainText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}
