using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TheIssueTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set;}

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        public int CompanyId { get; set; }

        //Image Properties
        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        //Navigation Properties
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual Company? Company { get; set; }
    }
}
