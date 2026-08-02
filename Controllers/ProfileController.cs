using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedCircle.Models;
using SharedCircle.ViewModels;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;

namespace SharedCircle.Controllers
{

    [Authorize]
    public class ProfileController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _db;


        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, ApplicationDbContext db)
        {
            _userManager = userManager;
            _environment = environment;
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }


            var followers = await _db.Follows.CountAsync(f => f.FollowingId == user.Id);

            var following = await _db.Follows.CountAsync(f => f.FollowerId == user.Id);
        


            ProfileVM vm = new ProfileVM
            {
                User = user,

                Posts = await _db.UserPosts
                    .Where(x => x.UserId == user.Id)
                    .Include(x => x.User)
                    .Include(x => x.Comments)
                        .ThenInclude(c => c.User)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(),

                FollowersCount = followers,
                FollowingCount = following,

                IsOwnProfile = true,
                IsFollowing = false
            };


            foreach (var post in vm.Posts)
            {
                post.IsLiked = await _db.Likes.AnyAsync(l =>
                    l.PostId == post.Id &&
                    l.UserId == user.Id);
            }


            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    throw new Exception("Logged in user not found");
                }



                string folder = Path.Combine(  _environment.WebRootPath,"images", "profile" );


                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }



                string fileName =  Guid.NewGuid().ToString()+ Path.GetExtension(image.FileName);
                string filePath = Path.Combine(folder, fileName);



                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }


                user.ProfileImage = "/images/profile/" + fileName;
                var result = await _userManager.UpdateAsync(user);


                if (!result.Succeeded)
                {
                    throw new Exception( string.Join(",", result.Errors.Select(e => e.Description))
                    );
                }


                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }



        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            EditProfileVM model = new EditProfileVM
            {
                FullName = user.FullName,
                Bio = user.Bio,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;
            user.Address = model.Address;
            user.DateOfBirth = model.DateOfBirth;

            await _userManager.UpdateAsync(user);

            TempData["success"] = "Profile updated successfully.";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ViewProfile(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound();

            var vm = new ProfileVM
            {
                User = user,

                Posts = await _db.UserPosts
                    .Where(x => x.UserId == id)
                    .Include(x => x.User)
                    .Include(x => x.Comments)
                        .ThenInclude(c => c.User)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(),

                FollowersCount = await _db.Follows
                    .CountAsync(f => f.FollowingId == id),

                FollowingCount = await _db.Follows
                    .CountAsync(f => f.FollowerId == id),

                IsOwnProfile = currentUser.Id == id,

                IsFollowing = currentUser.Id != id &&
                    await _db.Follows.AnyAsync(f =>
                        f.FollowerId == currentUser.Id &&
                        f.FollowingId == id)
            };

            foreach (var post in vm.Posts)
            {
                post.IsLiked = await _db.Likes.AnyAsync(l =>
                    l.PostId == post.Id &&
                    l.UserId == currentUser.Id);
            }

            return View("Index", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowers(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            
            var followers = await _db.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => new FollowUserVM
                {
                    Id = f.Follower.Id,
                    FullName = f.Follower.FullName,
                    ProfileImage = f.Follower.ProfileImage,

                    IsMe = f.Follower.Id == currentUser.Id,

                    IsFollowing = f.Follower.Id != currentUser.Id && _db.Follows.Any(x =>
                    x.FollowerId == currentUser.Id &&
                    x.FollowingId == f.Follower.Id)
                })
                .ToListAsync();


            return Json(followers);
        }


        [HttpGet]
        public async Task<IActionResult> GetFollowing(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var following = await _db.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => new FollowUserVM
                {
                    Id = f.Following.Id,
                    FullName = f.Following.FullName,
                    ProfileImage = f.Following.ProfileImage,

                    IsMe = f.Following.Id == currentUser.Id,
        
                    IsFollowing = f.Following.Id != currentUser.Id && _db.Follows.Any(x =>
                    x.FollowerId == currentUser.Id &&
                    x.FollowingId == f.Following.Id)
                })
                .ToListAsync();

            return Json(following);
        }

    }
}