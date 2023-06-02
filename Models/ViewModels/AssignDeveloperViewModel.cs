using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheIssueTracker.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DeveloperList { get; set; }
        public string? DeveloperId { get; set; }
    }


}
