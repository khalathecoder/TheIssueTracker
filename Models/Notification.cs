using System.ComponentModel.DataAnnotations;

namespace TheIssueTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int TicketId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        
        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }

        [Required]
        public int NotificationTypeId { get; set; }

        public bool HasBeenViewed { get; set; }


        //Navigation Properties
        public virtual NotificationType? NotificationType { get; set; }
        public virtual BTUser? Sender { get; set; }
        public virtual BTUser? Recipient { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
       
    }
}
