namespace OmniProduct_CoreDomain.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string Recipient { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string Channel { get; set; }         // "email", "sms", "push"
    public DateTime SentAt { get; set; }
}
