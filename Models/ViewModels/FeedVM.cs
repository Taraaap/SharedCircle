using SharedCircle.Models;

namespace SharedCircle.ViewModels
{
    public class FeedVM
    {
        public PostVM NewPost { get; set; } = new();

        public List<UserPost> Posts { get; set; } = new();
    }
}