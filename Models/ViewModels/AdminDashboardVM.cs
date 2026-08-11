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

        
        public List<TopPostVM> TopLikedPosts { get; set; } = new();
        public List<TopUserVM> TopActiveUsers { get; set; } = new();
        public List<TopUserVM> MostFollowedUsers { get; set; } = new();

        
        public List<RecentSignupVM> RecentSignups { get; set; } = new();

        public List<RecentPostVM> RecentPosts { get; set; } = new();

        public List<MonthComparisonVM> MonthComparisons { get; set; } = new();

        public int LockedUsersCount { get; set; }
    }

    public class DailyActivityVM
    {
        public DateTime Date { get; set; }
        public int Users { get; set; }
        public int Posts { get; set; }
        public int Comments { get; set; }
        public int Follows { get; set; }
    }

    public class TopPostVM
    {
        public int Id { get; set; }
        public string Caption { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string? AuthorImage { get; set; }
        public int LikeCount { get; set; }
    }

    public class TopUserVM
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfileImage { get; set; }
        public int Count { get; set; }
    }

    public class RecentSignupVM
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfileImage { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class RecentPostVM
    {
        public int Id { get; set; }
        public string Caption { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string? AuthorImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MonthComparisonVM
    {
        public string Metric { get; set; } = "";
        public int ThisMonth { get; set; }
        public int LastMonth { get; set; }
        public double PercentChange { get; set; }
    }
}