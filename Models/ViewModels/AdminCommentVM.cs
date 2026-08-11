namespace SharedCircle.ViewModels
{
    public class AdminCommentVM
    {
        public int Id { get; set; }

        public string Text { get; set; } = "";

        public DateTime CreatedAt { get; set; }

       
        public string AuthorId { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string? AuthorImage { get; set; }

        public int PostId { get; set; }
        public string PostCaption { get; set; } = "";
    }
}