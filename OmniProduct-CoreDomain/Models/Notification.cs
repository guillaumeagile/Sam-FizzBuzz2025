using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models;

[Table("Notifications")]
public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Recipient { get; set; }

    [MaxLength(256)]
    public string Subject { get; set; }

    public string Body { get; set; }

    [MaxLength(16)]
    public string Channel { get; set; }         // "email", "sms", "push"

    public DateTime SentAt { get; set; }

    // FK someone added expecting Notifications to be a real EF-tracked collection on Product.
    // Product.Notifications is [NotMapped] though, so this column exists in the DbContext but
    // is never actually set by anything.
    [MaxLength(64)]
    public string ProductId { get; set; }
}
