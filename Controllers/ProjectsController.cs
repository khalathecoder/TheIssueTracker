using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheIssueTracker.Data;
using TheIssueTracker.Extensions;
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
using TheIssueTracker.Models.ViewModels;
using TheIssueTracker.Services;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService fileService, IBTProjectService projectService, IBTTicketService ticketService, IBTRolesService rolesService, IBTCompanyService companyService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
  
            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());       
            return View(projects);
        }

		public async Task<IActionResult> IndexCopy()
		{

			List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
			return View(projects);
		}

		[HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project is null)
            {
                return NotFound();
            }

            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value, User.Identity!.GetCompanyId());

            AssignPMViewModel viewModel = new AssignPMViewModel()
            {
                Project = project,
                PMId = currentPM?.Id,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id)
                                  
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (viewModel.Project?.Id is not null)
            {
                //if unassigned is selected, remove the PM
                if(string.IsNullOrEmpty(viewModel.PMId))
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());
                }
                else  //else, add project manager
                {
                   
                    await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id, User.Identity!.GetCompanyId());
                }

                return RedirectToAction(nameof(Details), new {id = viewModel.Project!.Id});
            }

            return BadRequest();
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();          

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // GET: Projects/Create
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create()
        {
            List<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            Project project = new();
            return View(project);
        }


        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")] //using Enums class
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            ModelState.Remove("CompanyId");
            if (ModelState.IsValid)
            {
                BTUser? user = await _userManager.GetUserAsync(User);
                int companyId = User.Identity!.GetCompanyId();
                project.CompanyId = companyId;

                //Dates
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                //Images   
                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                //If user has role of project manager, add user
                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }

            List<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View(project);
        }


        // GET: Projects/Edit/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            List<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Images
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    //Dates
                    project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    await _projectService.UpdateProjectAsync(project, User.Identity!.GetCompanyId());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            List<ProjectPriority> priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View(project);
        }


        // GET: Projects/Delete/5
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            await _projectService.UpdateProjectAsync(project, User.Identity!.GetCompanyId());

            return View(project);
        }


        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            
            Project? project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());


            if (project != null) { 
            await _projectService.ArchiveProjectAsync(project, User.Identity!.GetCompanyId());
            }

            await _projectService.UpdateProjectAsync(project, User.Identity!.GetCompanyId());
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> MyProjects()
        {
            BTUser? user = await _userManager.GetUserAsync(User);
            return View(await _projectService.GetAllUserProjectsAsync(user!.Id));
        }

        public async Task<IActionResult> ArchivedProjects()
        {          
            return View(await _projectService.GetArchivedProjectsByCompanyIdAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> UnassignedProjects()
        {
            return View(await _projectService.GetUnassignedProjectsByCompanyIdAsync(User.Identity!.GetCompanyId()));
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            List<BTUser> members = await _companyService.GetCompanyMembersAsync(User.Identity!.GetCompanyId());
            List<BTUser> current = new();
            List<BTUser> unassigned = new();

            foreach (BTUser member in members)
            {
                if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
                {
                    if (project.Members.Contains(member))
                    {
                        current.Add(member);
                    }
                    else
                    {
                        unassigned.Add(member);
                    }
                }
            }

            AssignProjectMembersViewModel viewModel = new AssignProjectMembersViewModel()
            {
                Project = project,
                CurrentList = new SelectList(current, "Id", "FullName"),
                UnassignedList = new SelectList(unassigned, "Id", "FullName")
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(AssignProjectMembersViewModel viewModel)
        {
            if (viewModel.Project?.Id is not null)
            {
                Project? project = await _projectService.GetProjectByIdAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());

                if (project == null)
                {
                    return BadRequest();
                }

                List<BTUser> members = await _companyService.GetCompanyMembersAsync(User.Identity!.GetCompanyId());

                if (!string.IsNullOrEmpty(viewModel.MemberId))
                {
                    BTUser? member = members.FirstOrDefault(u => u.Id == viewModel.MemberId);
                    if (member is not null)
                    {
                        if (project.Members.Contains(member))
                        {
                            await _projectService.RemoveMemberFromProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }
                        else
                        {
                            await _projectService.AddMemberToProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }

                    }
                }

                List<BTUser> current = new();
                List<BTUser> unassigned = new();

                foreach (BTUser member in members)
                {
                    if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
                    {
                        if (project.Members.Contains(member))
                        {
                            current.Add(member);
                        }
                        else
                        {
                            unassigned.Add(member);
                        }
                    }
                }

                AssignProjectMembersViewModel newViewModel = new AssignProjectMembersViewModel()
                {
                    Project = project,
                    CurrentList = new SelectList(current, "Id", "FullName"),
                    UnassignedList = new SelectList(unassigned, "Id", "FullName")
                };

                return View(newViewModel);
            }

            return BadRequest();
        }
        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
