using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Models;
using SharedCircle.ViewModels;

namespace SharedCircle.ViewComponents
{
    [ViewComponent(Name = "RightSide")]
    public class RightSideViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RightSideViewComponent(ApplicationDbContext db,UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser == null)
            {
                return View(new List<SuggestedUserVM>());
            }


            // Get users that current user already follows
            var followingIds = await _db.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FollowingId)
                .ToListAsync();


            // Get users except current user and already followed users
            var users = await _db.Users
                .Where(u =>
                    u.Id != currentUser.Id &&
                    !followingIds.Contains(u.Id))
                .Take(5)
                .ToListAsync();


            return View(users);
        }
    }
}