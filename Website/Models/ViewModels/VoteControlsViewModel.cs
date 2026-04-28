using Website.Models;

namespace Website.Models.ViewModels;

public class VoteControlsViewModel
{
    public int ContentId { get; set; }
    public int Score { get; set; }
    public VoteType? UserVote { get; set; }
    public bool CanVote { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}