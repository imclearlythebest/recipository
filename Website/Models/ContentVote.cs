namespace Website.Models;

public class ContentVote
{
    public int Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public int ContentId { get; set; }
    public VoteType VoteType { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Content Content { get; set; } = null!;

}