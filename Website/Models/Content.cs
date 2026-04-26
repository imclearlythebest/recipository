namespace Website.Models;

public class Content
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public string MainText { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ContentVote> Votes { get; set; } = [];
    public int Upvotes => Votes?.Count(v => v.VoteType == VoteType.Upvote) ?? 0;
    public int Downvotes => Votes?.Count(v => v.VoteType == VoteType.Downvote) ?? 0;
    public int Score => Upvotes - Downvotes;
    public int? ParentId { get; set; }
    public Content? Parent { get; set; }
    public ICollection<Content> Replies { get; set; } = [];
}
