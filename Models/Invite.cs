using System.ComponentModel.DataAnnotations;

namespace TheIssueTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        public string? InviteeId { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }
        [Required]
        public string? InviteeFirstName { get; set; }
        [Required]
        public string? InviteeLastName { get; set; }


        //Navigation Properties
        public virtual ICollection<Company> Companies { get; set; } = new HashSet<Company>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual Invitor Invitor { get; set; }
        public virtual Invitee Invitee { get; set; }
    }
}
