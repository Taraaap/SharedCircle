namespace SharedCircle.ViewModels
{
    public class AdminFollowVM
    {
        public int Id { get; set; }

        public string FollowerId { get; set; } = "";
        public string FollowerName { get; set; } = "";
        public string? FollowerImage { get; set; }

        public string FollowingId { get; set; } = "";
        public string FollowingName { get; set; } = "";
        public string? FollowingImage { get; set; }
    }
}