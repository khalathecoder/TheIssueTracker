using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheIssueTracker.Extensions;
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
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
        private readonly UserManager<BTUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTTicketService ticketService, IBTCompanyService companyService, IBTRolesService roleService, IBTFileService fileService, UserManager<BTUser> userManager)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyService = companyService;
            _roleService = roleService;
            _fileService = fileService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
			DashboardViewModel model = new();
			int companyId = User.Identity!.GetCompanyId();

			model.Company = await _companyService.GetCompanyInfoAsync(companyId);
			model.Projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
												   .Where(p => p.Archived == false)
												   .ToList();

			model.Tickets = model.Projects
								 .SelectMany(p => p.Tickets)
								 .Where(t => t.Archived == false)
								 .ToList();

			model.Members = model.Company.Members.ToList();

			return View(model);
		}

		public IActionResult Landing()
		{
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> GglProjectTickets()
		{
			int companyId = User.Identity!.GetCompanyId();

			List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "ProjectName", "TicketCount" });

			foreach (Project prj in projects)
			{
				chartData.Add(new object[] { prj.Name!, prj.Tickets.Count() });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> GglProjectPriority()
		{
			int companyId = User.Identity!.GetCompanyId();

			List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "Priority", "Count" });


			foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
			{
				int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
				chartData.Add(new object[] { priority, priorityCount });
			}

			return Json(chartData);
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}