namespace Website.Models;

public class Follow
{
    public int Id { get; set; }
    public string FollowerId { get; set; } = null!;
    public ApplicationUser Follower { get; set; } = null!;
    public string FollowedId { get; set; } = null!;
    public ApplicationUser Followed { get; set; } = null!;
}