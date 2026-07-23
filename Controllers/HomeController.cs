using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.ViewModels;
using Microsoft.AspNetCore.Identity;
using SharedCircle.Models;
namespace SharedCircle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            return RedirectToAction("Feed");
        }



        [Authorize]
        public async Task<IActionResult> Feed()
        {
            FeedVM vm = new FeedVM();

            var currentUser = await _userManager.GetUserAsync(User);

            vm.Posts = await _db.UserPosts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            foreach (var post in vm.Posts)
            {
                post.IsLiked = await _db.Likes.AnyAsync(l =>
                    l.PostId == post.Id &&
                    l.UserId == currentUser.Id);
            }

            return View(vm);
        }
    }
}