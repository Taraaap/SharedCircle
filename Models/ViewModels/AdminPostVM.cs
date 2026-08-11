namespace SharedCircle.ViewModels
{
    public class AdminPostVM
    {
        public int Id { get; set; }
        public string Caption { get; set; } = "";
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string AuthorEmail { get; set; } = "";
        public string? AuthorImage { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
    }
}