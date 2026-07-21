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

            ProfileVM vm = new ProfileVM
            {
                User = user,
                Posts = await _db.UserPosts
                    .Where(x => x.UserId == user.Id)
                    .Include(x => x.User)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync()
            };

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
       
    }
}