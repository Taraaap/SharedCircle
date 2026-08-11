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
            var today = DateTime.Today;
            var startOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            var vm = new AdminDashboardVM
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalPosts = await _db.UserPosts.CountAsync(),
                TotalComments = await _db.Comments.CountAsync(),
                TotalFollows = await _db.Follows.CountAsync(),

                NewUsersToday = await _db.Users.CountAsync(u => u.JoinDate.Date == today),
                NewPostsToday = await _db.UserPosts.CountAsync(p => p.CreatedAt.Date == today),
                NewCommentsToday = await _db.Comments.CountAsync(c => c.CreatedAt.Date == today),
                NewFollowsToday = 0
            };

            // ===== Weekly activity =====
            var weekly = new List<DailyActivityVM>();

            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);

                weekly.Add(new DailyActivityVM
                {
                    Date = day,
                    Users = await _db.Users.CountAsync(u => u.JoinDate.Date == day),
                    Posts = await _db.UserPosts.CountAsync(p => p.CreatedAt.Date == day),
                    Comments = await _db.Comments.CountAsync(c => c.CreatedAt.Date == day),
                    Follows = 0
                });
            }

            vm.WeeklyActivity = weekly;

            // ===== Top liked posts =====
            vm.TopLikedPosts = await _db.UserPosts
                .Include(p => p.User)
                .Select(p => new TopPostVM
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    AuthorName = p.User.FullName,
                    AuthorImage = p.User.ProfileImage,
                    LikeCount = _db.Likes.Count(l => l.PostId == p.Id)
                })
                .OrderByDescending(p => p.LikeCount)
                .Take(5)
                .ToListAsync();

            // ===== Top active users (posts + comments) =====
            var userActivity = await _db.Users
                .Select(u => new TopUserVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    ProfileImage = u.ProfileImage,
                    Count = _db.UserPosts.Count(p => p.UserId == u.Id) + _db.Comments.Count(c => c.UserId == u.Id)
                })
                .OrderByDescending(u => u.Count)
                .Take(5)
                .ToListAsync();

            vm.TopActiveUsers = userActivity;

            // ===== Most followed users =====
            vm.MostFollowedUsers = await _db.Users
                .Select(u => new TopUserVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    ProfileImage = u.ProfileImage,
                    Count = _db.Follows.Count(f => f.FollowingId == u.Id)
                })
                .OrderByDescending(u => u.Count)
                .Take(5)
                .ToListAsync();

            // ===== Recent signups =====
            vm.RecentSignups = await _db.Users
                .OrderByDescending(u => u.JoinDate)
                .Take(5)
                .Select(u => new RecentSignupVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    ProfileImage = u.ProfileImage,
                    JoinDate = u.JoinDate
                })
                .ToListAsync();

            // ===== Recent activity feed (posts) =====
            vm.RecentPosts = await _db.UserPosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentPostVM
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    AuthorName = p.User.FullName,
                    AuthorImage = p.User.ProfileImage,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            // ===== Month-over-month comparison =====
            int usersThisMonth = await _db.Users.CountAsync(u => u.JoinDate >= startOfThisMonth);
            int usersLastMonth = await _db.Users.CountAsync(u => u.JoinDate >= startOfLastMonth && u.JoinDate < startOfThisMonth);

            int postsThisMonth = await _db.UserPosts.CountAsync(p => p.CreatedAt >= startOfThisMonth);
            int postsLastMonth = await _db.UserPosts.CountAsync(p => p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfThisMonth);

            int commentsThisMonth = await _db.Comments.CountAsync(c => c.CreatedAt >= startOfThisMonth);
            int commentsLastMonth = await _db.Comments.CountAsync(c => c.CreatedAt >= startOfLastMonth && c.CreatedAt < startOfThisMonth);

            double PercentChange(int current, int previous)
            {
                if (previous == 0) return current > 0 ? 100 : 0;
                return Math.Round(((double)(current - previous) / previous) * 100, 1);
            }

            vm.MonthComparisons = new List<MonthComparisonVM>
    {
        new MonthComparisonVM { Metric = "Users", ThisMonth = usersThisMonth, LastMonth = usersLastMonth, PercentChange = PercentChange(usersThisMonth, usersLastMonth) },
        new MonthComparisonVM { Metric = "Posts", ThisMonth = postsThisMonth, LastMonth = postsLastMonth, PercentChange = PercentChange(postsThisMonth, postsLastMonth) },
        new MonthComparisonVM { Metric = "Comments", ThisMonth = commentsThisMonth, LastMonth = commentsLastMonth, PercentChange = PercentChange(commentsThisMonth, commentsLastMonth) }
    };

            // ===== Moderation snapshot =====
            var allUsers = await _userManager.Users.ToListAsync();
            int lockedCount = 0;

            foreach (var u in allUsers)
            {
                if (await _userManager.IsLockedOutAsync(u))
                    lockedCount++;
            }

            vm.LockedUsersCount = lockedCount;

            return View(vm);
        }


        //user
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




        //post

        public async Task<IActionResult> Posts(string term)
        {
            var query = _db.UserPosts
                .Include(p => p.User)
                .Where(p =>
                    string.IsNullOrEmpty(term) ||
                    p.Caption.Contains(term) ||
                    p.User.FullName.Contains(term));

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new AdminPostVM
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    AuthorId = p.UserId,
                    AuthorName = p.User.FullName,
                    AuthorEmail = p.User.Email!,
                    AuthorImage = p.User.ProfileImage,
                    CommentCount = _db.Comments.Count(c => c.PostId == p.Id),
                    LikeCount = _db.Likes.Count(l => l.PostId == p.Id)
                })
                .ToListAsync();

            ViewBag.SearchTerm = term;

            return View(posts);
        }

        public async Task<IActionResult> PostDetails(int id)
        {
            var post = await _db.UserPosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var vm = new AdminPostVM
            {
                Id = post.Id,
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                AuthorId = post.UserId,
                AuthorName = post.User.FullName,
                AuthorEmail = post.User.Email!,
                AuthorImage = post.User.ProfileImage,
                CommentCount = await _db.Comments.CountAsync(c => c.PostId == id),
                LikeCount = await _db.Likes.CountAsync(l => l.PostId == id)
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _db.UserPosts.FindAsync(id);

            if (post == null)
            {
                TempData["error"] = "Post not found.";
                return RedirectToAction("Posts");
            }

           
            var likes = await _db.Likes
                .Where(l => l.PostId == id)
                .ToListAsync();

            _db.Likes.RemoveRange(likes);

        
            var comments = await _db.Comments
                .Where(c => c.PostId == id)
                .ToListAsync();

            _db.Comments.RemoveRange(comments);

           
            var notifications = await _db.Notifications
                .Where(n => n.PostId == id)
                .ToListAsync();

            _db.Notifications.RemoveRange(notifications);

          
            _db.UserPosts.Remove(post);

            await _db.SaveChangesAsync();

            TempData["success"] = "Post deleted successfully.";

            return RedirectToAction("Posts");
        }

        //comments 
        public async Task<IActionResult> Comments(string term)
        {
            var query = _db.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(c =>
                    c.Text.Contains(term) ||
                    c.User.FullName.Contains(term) ||
                    c.Post.Caption.Contains(term));
            }

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AdminCommentVM
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,

                    AuthorId = c.UserId,
                    AuthorName = c.User.FullName,
                    AuthorImage = c.User.ProfileImage,

                    PostId = c.PostId,
                    PostCaption = c.Post.Caption
                })
                .ToListAsync();

            ViewBag.SearchTerm = term;

            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _db.Comments.FindAsync(id);

            if (comment == null)
            {
                TempData["error"] = "Comment not found.";
                return RedirectToAction(nameof(Comments));
            }

            _db.Comments.Remove(comment);

            await _db.SaveChangesAsync();

            TempData["success"] = "Comment deleted successfully.";

            return RedirectToAction(nameof(Comments));
        }

        //follow 
        public async Task<IActionResult> Follows()
        {
            // Total follow relationships
            var totalFollows = await _db.Follows.CountAsync();

            // Total users
            var totalUsers = await _db.Users.CountAsync();

            // Most followed users
            var mostFollowedUsers = await _db.Users
                .Select(u => new AdminFollowUserVM
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    ProfileImage = u.ProfileImage,

                    FollowerCount = _db.Follows
                        .Count(f => f.FollowingId == u.Id),

                    FollowingCount = _db.Follows
                        .Count(f => f.FollowerId == u.Id)
                })
                .OrderByDescending(u => u.FollowerCount)
                .Take(10)
                .ToListAsync();

            // All follow relationships
            var relationships = await _db.Follows
                .Include(f => f.Follower)
                .Include(f => f.Following)
                .OrderByDescending(f => f.Id)
                .Select(f => new AdminFollowVM
                {
                    Id = f.Id,

                    FollowerId = f.FollowerId,
                    FollowerName = f.Follower.FullName,
                    FollowerImage = f.Follower.ProfileImage,

                    FollowingId = f.FollowingId,
                    FollowingName = f.Following.FullName,
                    FollowingImage = f.Following.ProfileImage
                })
                .ToListAsync();

            var vm = new AdminFollowOverviewVM
            {
                TotalFollows = totalFollows,
                TotalUsers = totalUsers,
                MostFollowedUsers = mostFollowedUsers,
                Relationships = relationships
            };

            return View(vm);
        }
    }
}