using SharedCircle.Models;

public class ProfileVM
{
    public ApplicationUser User { get; set; }

    public List<UserPost> Posts { get; set; } = new();

    public int FollowersCount { get; set; }

    public int FollowingCount { get; set; }
    public bool IsOwnProfile { get; set; }
    public bool IsFollowing { get; set; }
}