using System.ComponentModel.DataAnnotations;

namespace SharedCircle.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

       
        public int PostId { get; set; }
        public UserPost Post { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}