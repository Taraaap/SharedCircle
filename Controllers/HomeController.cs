using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.ViewModels;

namespace SharedCircle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
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

            vm.Posts = await _db.UserPosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();


            return View(vm);
        }
    }
}