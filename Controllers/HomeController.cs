using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheIssueTracker.Extensions;
using TheIssueTracker.Models;
using TheIssueTracker.Models.ViewModels;
using TheIssueTracker.Services;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _roleService;
        private readonly IBTFileService _fileService;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTTicketService ticketService, IBTCompanyService companyService, IBTRolesService roleService, IBTFileService fileService)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyService = companyService;
            _roleService = roleService;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            DashboardViewModel viewModel = new DashboardViewModel();
            int? companyId = User.Identity!.GetCompanyId();
            
            return View();
        }

		public IActionResult Landing()
		{
			return View();
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}