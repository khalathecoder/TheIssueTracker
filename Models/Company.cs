using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheIssueTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }

        //Image Properties
        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        //Navigation properties
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
