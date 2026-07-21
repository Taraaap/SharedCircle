using SharedCircle.Models;

public class Like
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public UserPost Post { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}