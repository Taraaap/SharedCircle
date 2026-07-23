using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedCircle.Models
{
    public class UserPost
    {
        public int Id { get; set; }

        [Required]
        public string? Caption { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public List<Comment> Comments { get; set; } = new();
        [NotMapped]
        public bool IsLiked { get; set; }
    }
}