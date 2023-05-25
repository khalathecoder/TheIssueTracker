using System.ComponentModel.DataAnnotations;

namespace TheIssueTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Invite Date")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Join Date")]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; }
        public int? ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }

        [Required]
        [Display(Name = "Invitee Email")]
        [DataType(DataType.EmailAddress)]
        public string? InviteeEmail { get; set; }

        [Required]

        [Display(Name = "Invitee First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }


        [Required]
        [Display(Name = "Invitee Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }


        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }
    }
}
