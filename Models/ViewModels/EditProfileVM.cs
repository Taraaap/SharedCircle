using System.ComponentModel.DataAnnotations;

namespace SharedCircle.ViewModels
{
    public class EditProfileVM
    {
        [Required]
        public string FullName { get; set; }

        public string? Bio { get; set; }

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }
    }
}