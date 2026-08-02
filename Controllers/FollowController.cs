using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Hubs;
using SharedCircle.Models;
using Microsoft.AspNetCore.SignalR;
using SharedCircle.Hubs;

namespace SharedCircle.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public FollowController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,IHubContext<NotificationHub>notificationHub)
        {
            _db = db;
            _userManager = userManager;
            _notificationHub = notificationHub;
        }


        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            if (currentUser.Id == userId)
                return BadRequest();

            var follow = await _db.Follows.FirstOrDefaultAsync(x =>
                x.FollowerId == currentUser.Id &&
                x.FollowingId == userId);

            bool isFollowing;

            if (follow == null)
            {
                _db.Follows.Add(new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId
                });

                _db.Notifications.Add(new Notification
                {
                    SenderId = currentUser.Id,
                    ReceiverId = userId,
                    Message = "started  to following you"
                });

                isFollowing = true;
            }
            else
            {
                _db.Follows.Remove(follow);

                isFollowing = false;
            }

            await _db.SaveChangesAsync();

            var targetFollowers = await _db.Follows.CountAsync(f => f.FollowingId == userId);
            var targetFollowing = await _db.Follows.CountAsync(f => f.FollowerId == userId);

            var myFollowers = await _db.Follows.CountAsync(f => f.FollowingId == currentUser.Id);
            var myFollowing = await _db.Follows.CountAsync(f => f.FollowerId == currentUser.Id);

            

            await _notificationHub.Clients.Group(userId)
                .SendAsync("ReceiveNotification");

            return Ok(new
            {
                isFollowing,
                followersCount = targetFollowers,
                followingCount = targetFollowing
            });
        }





    }
}