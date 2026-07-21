using SharedCircle.Models;

public class ProfileVM
{
    public ApplicationUser User { get; set; }

    public List<UserPost> Posts { get; set; } = new();
}