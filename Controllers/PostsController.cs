using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Models;
using SharedCircle.ViewModels;

using Microsoft.AspNetCore.SignalR;
using SharedCircle.Hubs;

namespace SharedCircle.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IHubContext<CommentHub> _hub;   //  this line for Like SignalR
        public PostsController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            IHubContext<NotificationHub> notificationHub,
            IHubContext<CommentHub> hub)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
            _notificationHub = notificationHub;
            _hub = hub;
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(FeedVM model)
        {

            var user = await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Unauthorized();
            }
            if (string.IsNullOrWhiteSpace(model.NewPost.Caption) && model.NewPost.Image == null)
            {
                TempData["error"] = "Please enter a caption or select an image.";
                return RedirectToAction("Feed", "Home");
            }

            string? imagePath = null;



            if (model.NewPost.Image != null)
            {

                string folder = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "posts"
                );


                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }



                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.NewPost.Image.FileName);



                string filePath =
                    Path.Combine(folder, fileName);



                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewPost.Image.CopyToAsync(stream);
                }


                imagePath = "/images/posts/" + fileName;

            }



            UserPost post = new UserPost
            {
                Caption = model.NewPost.Caption,
                ImageUrl = imagePath,
                CreatedAt = DateTime.Now,
                UserId = user.Id
            };


            _db.UserPosts.Add(post);

            await _db.SaveChangesAsync();


            TempData["success"] = "Post created successfully!";


            return RedirectToAction("Feed", "Home");

        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var post = await _db.UserPosts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }


            if (post.UserId != user.Id)
            {
                return Unauthorized();
            }


            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                string imagePath = Path.Combine(
                    _environment.WebRootPath,
                    post.ImageUrl.TrimStart('/').Replace("/", "\\")
                );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _db.UserPosts.Remove(post);
            await _db.SaveChangesAsync();

            TempData["success"] = "Post deleted successfully.";

            return RedirectToAction("Index", "Profile");
        }



        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            var like = await _db.Likes
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == user.Id);

            var post = await _db.UserPosts.FindAsync(postId);

            bool isLiked;

            if (like == null)
            {
                _db.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = user.Id
                });

                if (post.UserId != user.Id)
                {
                    _db.Notifications.Add(new Notification
                    {
                        SenderId = user.Id,
                        ReceiverId = post.UserId,
                        PostId = post.Id,
                        Message = "liked your post"
                    });
                }

                post.LikeCount++;
                isLiked = true;
            }
            else
            {
                _db.Likes.Remove(like);

                if (post.LikeCount > 0)
                {
                    post.LikeCount--;
                }

                isLiked = false;
            }

            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync( "ReceiveLike",post.Id,post.LikeCount);

            await _notificationHub.Clients.Group(post.UserId).SendAsync("ReceiveNotification");

            return Ok(new
            {
                likes = post.LikeCount,
                isLiked = isLiked
            });
        }
    }

}
