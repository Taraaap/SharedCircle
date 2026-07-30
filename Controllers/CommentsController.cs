using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Helpers;
using SharedCircle.Hubs;
using SharedCircle.Models;
using SharedCircle.ViewModels;


[Authorize]
public class CommentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<CommentHub> _hub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    public CommentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IHubContext<CommentHub> hub,
        IHubContext<NotificationHub> notificationHub)
    {
        _db = db;
        _userManager = userManager;
        _hub = hub;
        _notificationHub = notificationHub;
    }

    [HttpPost]
  
    public async Task<IActionResult> Add(int postId, string text, string connectionId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();


        if (string.IsNullOrWhiteSpace(text))
            return BadRequest();


        var comment = new Comment
        {
            PostId = postId,
            UserId = user.Id,
            Text = text,
            CreatedAt = DateTime.Now
        };


        var post = await _db.UserPosts.FindAsync(postId);

        if (post == null)
            return NotFound();


        post.CommentCount++;


        _db.Comments.Add(comment);

        if (post.UserId != user.Id)
        {
            _db.Notifications.Add(new Notification
            {
                SenderId = user.Id,
                ReceiverId = post.UserId,
                PostId = post.Id,
                Message = "commented on your post"
            });
        }

        await _db.SaveChangesAsync();
        await _notificationHub.Clients.Group(post.UserId).SendAsync("ReceiveNotification");

        await _hub.Clients.AllExcept(connectionId).SendAsync("ReceiveComment",
         comment.PostId,
         user.FullName,
         user.ProfileImage ?? "/images/default-profile.png",
         comment.Text,
         comment.CreatedAt.ToString("hh:mm tt"),
         post.CommentCount
     );


        return Ok(new
        {
            comments = post.CommentCount,
            user = user.FullName,
            profileImage = user.ProfileImage ?? "/images/default-profile.png",
            text = comment.Text,
            time = TimeHelper.GetTimeAgo(comment.CreatedAt)
        });
    }
  
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }


        var comment = await _db.Comments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == id);


        if (comment == null)
        {
            return NotFound();
        }


     
        if (comment.UserId != user.Id)
        {
            return Unauthorized();
        }


        var post = await _db.UserPosts
            .FirstOrDefaultAsync(x => x.Id == comment.PostId);


        if (post != null && post.CommentCount > 0)
        {
            post.CommentCount--;
        }


        _db.Comments.Remove(comment);


        await _db.SaveChangesAsync();


        TempData["success"] = "Comment deleted successfully.";


        return RedirectToAction("Feed", "Home");
    }
}
