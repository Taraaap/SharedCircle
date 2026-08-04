using Microsoft.AspNetCore.SignalR;

namespace SharedCircle.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, string senderName, string message)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveMessage", senderName, message);
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}