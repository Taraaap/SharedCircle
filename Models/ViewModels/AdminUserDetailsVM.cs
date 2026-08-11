namespace SharedCircle.ViewModels
{
    public class AdminUserDetailsVM
    {
        public string Id { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? Bio { get; set; }

        public string? ProfileImage { get; set; }

        public DateTime JoinDate { get; set; }

        public string Role { get; set; } = "User";

        public bool IsLocked { get; set; }
        public int PostCount { get; set; }
    }
}