using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheIssueTracker.Models.ViewModels
{
    public class AssignProjectMembersViewModel
    {
        public Project? Project { get; set; }
        public SelectList? UnassignedList { get; set; }
        public SelectList? CurrentList { get; set; }
        public string? MemberId { get; set; }
    }
}
