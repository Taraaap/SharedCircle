namespace SharedCircle.ViewModels
{
    public class AdminUserVM
    {
        public string Id { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? ProfileImage { get; set; }

        public DateTime JoinDate { get; set; }

        public string Role { get; set; } = "User";

        public bool IsLocked { get; set; }
    }
}