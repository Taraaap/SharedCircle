namespace SharedCircle.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalFollows { get; set; }

     
        public int NewUsersToday { get; set; }
        public int NewPostsToday { get; set; }
        public int NewCommentsToday { get; set; }
        public int NewFollowsToday { get; set; }

        public List<DailyActivityVM> WeeklyActivity { get; set; } = new();
    }

    public class DailyActivityVM
    {
        public DateTime Date { get; set; }

        public int Users { get; set; }

        public int Posts { get; set; }

        public int Comments { get; set; }

        public int Follows { get; set; }
    }
}