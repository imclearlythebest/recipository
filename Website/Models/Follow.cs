namespace Website.Models;

public class Follow
{
    public int Id { get; set; }
    public Guid FollowerId { get; set; }
    public ApplicationUser Follower { get; set; } = null!;
    public Guid FollowedId { get; set; }
    public ApplicationUser Followed { get; set; } = null!;
}