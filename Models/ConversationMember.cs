using SharedCircle.Models;

public class ConversationMember
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; }

    public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    public int? LastReadMessageId { get; set; }
    public int UnreadCount { get; set; } = 0;


}