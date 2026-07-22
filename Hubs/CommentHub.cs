using Microsoft.AspNetCore.SignalR;

namespace SharedCircle.Hubs
{
    public class CommentHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}