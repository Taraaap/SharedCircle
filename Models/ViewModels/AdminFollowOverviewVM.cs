namespace SharedCircle.ViewModels
{
    public class AdminFollowOverviewVM
    {
        public int TotalFollows { get; set; }

        public int TotalUsers { get; set; }

        public List<AdminFollowUserVM> MostFollowedUsers { get; set; }
            = new();

        public List<AdminFollowVM> Relationships { get; set; }
            = new();
    }
}