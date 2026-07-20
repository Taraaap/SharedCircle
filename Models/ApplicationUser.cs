using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SharedCircle.Models
{
    public class ApplicationUser : IdentityUser
    {
        //[Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$",
            ErrorMessage = "Full name can only contain letters and spaces")]
        public string? FullName { get; set; }

        public string? Bio { get; set; }

        public string? ProfileImage { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;


        public string? Address { get; set; }


        public DateOnly? DateOfBirth { get; set; }

    }
}