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
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> chatHub)
        {
            _db = db;
            _userManager = userManager;
            _chatHub = chatHub;
        }

        public IActionResult Index()
        {
            return View();
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
                    c.Members.Any(m => m.UserId == currentUser.Id) &&
                    c.Members.Any(m => m.UserId == userId));

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
                    });

                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                conversationId = conversation.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string text)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender == null)
                return Unauthorized();

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = sender.Id,
                Text = text
            };

            _db.Messages.Add(message);

            await _db.SaveChangesAsync();

            // We'll notify everyone in the conversation in the next step

            return Ok(new
            {
                sender = sender.FullName,
                text,
                time = message.SentAt.ToString("hh:mm tt")
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var users = await _db.Users
                .Where(x =>
                    x.Id != currentUser.Id &&
                    (string.IsNullOrEmpty(term) ||
                     x.FullName.Contains(term)))
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.ProfileImage
                })
                .ToListAsync();

            return Json(users);
        }
    }
}
