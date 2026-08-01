using SharedCircle.Models;

namespace SharedCircle.ViewModels
{
    public class SuggestedUserVM
    {
        public ApplicationUser User { get; set; }

        public bool IsFollowing { get; set; }
    }
}