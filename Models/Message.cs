using SharedCircle.Models;

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; }

    public string SenderId { get; set; }

    public ApplicationUser Sender { get; set; }

    public string Text { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;
}