namespace Website.Models;

public class Content
{
    public int Id { get; set; }
    public required string MainText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ContentVote> Votes { get; set; } = [];
    public int Upvotes => Votes?.Count(v => v.VoteType == VoteType.Upvote) ?? 0;
    public int Downvotes => Votes?.Count(v => v.VoteType == VoteType.Downvote) ?? 0;
    public int Score => Upvotes - Downvotes;
}
