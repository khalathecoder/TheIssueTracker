using System.ComponentModel.DataAnnotations;

namespace TheIssueTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int TicketId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
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
