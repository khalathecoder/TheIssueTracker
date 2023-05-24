using System.ComponentModel.DataAnnotations;

namespace TheIssueTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [Display(Name = "Ticket Id")]
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
