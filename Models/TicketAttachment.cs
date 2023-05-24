using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheIssueTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public int TickeId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        //Image Properties
        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }


        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
