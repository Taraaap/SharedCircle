using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SharedCircle.ViewModels
{
    public class PostVM
    {
        [Required]
        public string? Caption { get; set; }

        public IFormFile? Image { get; set; }
    }
}