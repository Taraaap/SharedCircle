using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedCircle.Data;
using SharedCircle.Hubs;
using SharedCircle.Models;
using Microsoft.EntityFrameworkCore;
namespace SharedCircle.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.CurrentUserId = currentUser?.Id;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var users = await _db.Users
                .Where(x =>
                    x.Id != currentUser.Id &&
                    (string.IsNullOrEmpty(term) ||
                     x.FullName!.Contains(term)))
                .Select(x => new
                {
                    id = x.Id,
                    fullName = x.FullName,
                    profileImage = x.ProfileImage
                })
                .ToListAsync();

            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> StartConversation(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var conversation = await _db.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c =>
                    c.Members.Count == 2 &&
                    c.Members.Any(x => x.UserId == currentUser.Id) &&
                    c.Members.Any(x => x.UserId == userId));

            if (conversation == null)
            {
                conversation = new Conversation();

                _db.Conversations.Add(conversation);

                await _db.SaveChangesAsync();

                _db.ConversationMembers.AddRange(
                    new ConversationMember
                    {
                        ConversationId = conversation.Id,
                        UserId = currentUser.Id
                    },
                    new ConversationMember
                    {
                        ConversationId = conversation.Id,
                        UserId = userId
                    }
                );

                await _db.SaveChangesAsync();
            }

            return Json(new
            {
                conversationId = conversation.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(
            int conversationId,
            string text)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Message is empty.");

            var isMember = await _db.ConversationMembers
                .AnyAsync(x =>
                    x.ConversationId == conversationId &&
                    x.UserId == currentUser.Id);

            if (!isMember)
                return Forbid();

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUser.Id,
                Text = text,
                SentAt = DateTime.Now
            };

            _db.Messages.Add(message);

            await _db.SaveChangesAsync();

            return Json(new
            {
                id = message.Id,
                senderId = currentUser.Id,
                sender = currentUser.FullName,
                text = message.Text,
                time = message.SentAt.ToString("hh:mm tt")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var isMember = await _db.ConversationMembers
                .AnyAsync(x =>
                    x.ConversationId == conversationId &&
                    x.UserId == currentUser.Id);

            if (!isMember)
                return Forbid();

            var messages = await _db.Messages
                .Where(x => x.ConversationId == conversationId)
                .OrderBy(x => x.SentAt)
                .Select(x => new
                {
                    id = x.Id,
                    senderId = x.SenderId,
                    sender = x.Sender!.FullName,
                    text = x.Text,
                    time = x.SentAt.ToString("hh:mm tt")
                })
                .ToListAsync();

            return Json(messages);
        }
    }
}
