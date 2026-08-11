using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Models;
using SharedCircle.ViewModels;

namespace SharedCircle.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardVM
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalPosts = await _db.UserPosts.CountAsync(),
                TotalComments = await _db.Comments.CountAsync(),
                TotalFollows = await _db.Follows.CountAsync()
            };

            return View(vm);
        }


        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.JoinDate)
                .ToListAsync();

            var model = new List<AdminUserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var isLocked =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                model.Add(new AdminUserVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    ProfileImage = user.ProfileImage,
                    JoinDate = user.JoinDate,
                    Role = roles.FirstOrDefault() ?? "User",
                    IsLocked = isLocked
                });
            }

            return View(model);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var postCount = await _db.UserPosts
                .CountAsync(p => p.UserId == user.Id);

            var commentCount = await _db.Comments
                .CountAsync(c => c.UserId == user.Id);

            var model = new AdminUserDetailsVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage,
                JoinDate = user.JoinDate,

                Role = roles.FirstOrDefault() ?? "User",

                IsLocked = user.LockoutEnd.HasValue &&
                           user.LockoutEnd.Value > DateTimeOffset.UtcNow,

                PostCount = postCount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

           
            if (id == currentUserId)
            {
                TempData["error"] = "You cannot change your own admin role.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            if (role != "Admin" && role != "User")
            {
                TempData["error"] = "Invalid role.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

                if (!removeResult.Succeeded)
                {
                    TempData["error"] = "Unable to change user role.";
                    return RedirectToAction(nameof(UserDetails), new { id });
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);

            if (!addResult.Succeeded)
            {
                TempData["error"] = "Unable to assign the new role.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            TempData["success"] = $"User role changed to {role}.";

            return RedirectToAction(nameof(UserDetails), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

           
            if (id == currentUserId)
            {
                TempData["error"] = "You cannot lock your own account.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["error"] = "Unable to lock this account.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            TempData["success"] = "User account locked successfully.";

            return RedirectToAction(nameof(UserDetails), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["error"] = "Unable to unlock this account.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            TempData["success"] = "User account unlocked successfully.";

            return RedirectToAction(nameof(UserDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

         
            if (id == currentUserId)
            {
                TempData["error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["error"] = "Unable to delete this user.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            TempData["success"] = "User account deleted successfully.";

            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new AdminCreateUserVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Role != "User" && model.Role != "Admin")
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
                return View(model);
            }

          
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                JoinDate = DateTime.Now,

               
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            TempData["success"] =
                $"User '{model.FullName}' created successfully as {model.Role}.";

            return RedirectToAction(nameof(Users));
        }

    }
}