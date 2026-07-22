namespace SharedCircle.Helpers
{
    public static class TimeHelper
    {
        public static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;

            if (span.TotalSeconds < 60)
                return "Just now";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} min ago";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hr ago";

            if (span.TotalDays < 2)
                return "Yesterday";

            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays} days ago";

            if (span.TotalDays < 30)
                return $"{(int)(span.TotalDays / 7)} weeks ago";

            if (span.TotalDays < 365)
                return $"{(int)(span.TotalDays / 30)} months ago";

            return $"{(int)(span.TotalDays / 365)} years ago";
        }
    }
}