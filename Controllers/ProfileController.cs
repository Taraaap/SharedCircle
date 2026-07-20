using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedCircle.Models;

namespace SharedCircle.Controllers
{

    [Authorize]
    public class ProfileController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;



        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }



        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
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



                string folder = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "profile"
                );


                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }



                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(image.FileName);



                string filePath =
                    Path.Combine(folder, fileName);



                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }



                user.ProfileImage = "/images/profile/" + fileName;


                var result = await _userManager.UpdateAsync(user);


                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(",", result.Errors.Select(e => e.Description))
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


    }
}