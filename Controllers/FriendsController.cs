using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Models;

namespace SharedCircle.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendsController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var friends = await _db.Follows
                .Where(f =>
                    f.FollowerId == currentUser.Id &&
                    _db.Follows.Any(reverse =>
                        reverse.FollowerId == f.FollowingId &&
                        reverse.FollowingId == currentUser.Id))
                .Include(f => f.Following)
                .Select(f => f.Following)
                .ToListAsync();

            return View(friends);
        }
    }
}