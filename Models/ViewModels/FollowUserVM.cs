using SharedCircle.Models;

namespace SharedCircle.ViewModels
{
    public class FollowUserVM
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string? ProfileImage { get; set; }

        public bool IsFollowing { get; set; }
    }
}