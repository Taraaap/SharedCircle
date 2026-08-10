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
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> chatHub )
        {
            _db = db;
            _userManager = userManager;
            _chatHub = chatHub;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.CurrentUserId = currentUser?.Id;

            return View();
        }
       
       
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var query = _db.Users
                .Where(x =>
                    x.Id != currentUser.Id &&
                    (string.IsNullOrEmpty(term) ||
                     x.FullName!.Contains(term)))
                .Select(x => new
                {
                    id = x.Id,
                    fullName = x.FullName,
                    profileImage = x.ProfileImage,

                    conversationId = _db.ConversationMembers
                        .Where(cm =>
                            cm.UserId == x.Id &&
                            _db.ConversationMembers.Any(cm2 =>
                                cm2.ConversationId == cm.ConversationId &&
                                cm2.UserId == currentUser.Id))
                        .Select(cm => cm.ConversationId)
                        .FirstOrDefault()
                });

           
            if (string.IsNullOrEmpty(term))
            {
                query = query.Where(x =>
                    x.conversationId != 0 &&
                    _db.Messages.Any(m => m.ConversationId == x.conversationId));
            }

            var users = await query.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var currentMember = await _db.ConversationMembers
                    .FirstOrDefaultAsync(cm =>
                        cm.ConversationId == user.conversationId &&
                        cm.UserId == currentUser.Id);

                var lastReadMessageId =
                    currentMember?.LastReadMessageId ?? 0;

                var lastMessage = await _db.Messages
                    .Where(m => m.ConversationId == user.conversationId)
                    .OrderByDescending(m => m.Id)
                    .Select(m => new
                    {
                        m.Id,
                        m.Text,
                        m.SenderId,
                        m.SentAt
                    })
                    .FirstOrDefaultAsync();

                var unreadCount = await _db.Messages
                    .CountAsync(m =>
                        m.ConversationId == user.conversationId &&
                        m.Id > lastReadMessageId &&
                        m.SenderId != currentUser.Id);

                result.Add(new
                {
                    user.id,
                    user.fullName,
                    user.profileImage,

                    conversationId = user.conversationId,

                    lastMessage = lastMessage?.Text,
                    lastMessageSenderId = lastMessage?.SenderId,
                    lastMessageTime = lastMessage?.SentAt,

                    unreadCount = unreadCount
                });
            }

            return Json(result);
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
            var sender = await _userManager.GetUserAsync(User);

            if (sender == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(text))
                return BadRequest();

            
            var receiverId = await _db.ConversationMembers
                .Where(x =>
                    x.ConversationId == conversationId &&
                    x.UserId != sender.Id)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync();

            if (receiverId == null)
                return BadRequest("Receiver not found.");

           
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = sender.Id,
                Text = text,
                SentAt = DateTime.Now
            };

            _db.Messages.Add(message);

            var receiver = await _db.ConversationMembers
     .Where(cm =>
         cm.ConversationId == conversationId &&
         cm.UserId != sender.Id)
     .FirstOrDefaultAsync();

            if (receiver != null)
            {
                receiver.UnreadCount++;

                await _db.SaveChangesAsync();
            }

            await _chatHub.Clients
                .Group(conversationId.ToString())
                .SendAsync(
                    "ReceiveMessage",
                    new
                    {
                        id = message.Id,
                        conversationId = message.ConversationId,
                        senderId = sender.Id,
                        sender = sender.FullName,
                        text = message.Text,
                        time = message.SentAt.ToString("hh:mm tt"),
                        sentAt = message.SentAt
                    }
                );

            await _chatHub.Clients
                .User(receiverId)
                .SendAsync(
                    "UnreadMessage",
                    new
                    {
                        conversationId = conversationId
                    }
                );

            return Ok(new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderId = sender.Id,
                sender = sender.FullName,
                text = message.Text,
                time = message.SentAt.ToString("hh:mm tt"),
                sentAt = message.SentAt
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
                    time = x.SentAt.ToString("hh:mm tt"),
                    sentAt = x.SentAt

                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var member = await _db.ConversationMembers
                .FirstOrDefaultAsync(cm =>
                    cm.ConversationId == conversationId &&
                    cm.UserId == currentUser.Id);

            if (member == null) return Forbid();

            var lastMessageId = await _db.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

            member.LastReadMessageId = lastMessageId;
            member.UnreadCount = 0;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var count = await _db.ConversationMembers
                .CountAsync(x =>
                    x.UserId == currentUser.Id &&
                    x.UnreadCount > 0);

            return Json(new
            {
                count
            });
        }
    }
}
