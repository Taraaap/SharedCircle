using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCircle.Data;
using SharedCircle.Models;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> GetNotifications()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var notifications = await _db.Notifications
            .Where(n => n.ReceiverId == user.Id)
            .Include(n => n.Sender)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Json(notifications);
    }

    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var count = await _db.Notifications
            .CountAsync(n => n.ReceiverId == user.Id && !n.IsRead);

        return Json(count);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var notifications = await _db.Notifications
            .Where(n => n.ReceiverId == user.Id && !n.IsRead)
            .ToListAsync();

        foreach (var item in notifications)
        {
            item.IsRead = true;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }
}