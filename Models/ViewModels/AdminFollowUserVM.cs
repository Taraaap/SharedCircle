namespace SharedCircle.ViewModels
{
    public class AdminFollowUserVM
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfileImage { get; set; }

        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
    }
}