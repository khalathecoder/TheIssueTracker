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
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            BTUser? user = await _userManager.GetUserAsync(User);        

            var applicationDbContext = _context.Projects.Where(p => p.CompanyId == user!.CompanyId && p.Archived == false)
                                                .Include(p => p.Company)
                                                .Include(p => p.ProjectPriority);

            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            BTUser? user = await _userManager.GetUserAsync(User);

            var project = await _context.Projects.Where(p => p.CompanyId == user!.CompanyId && p.Archived == false)
                                        .Include(p => p.Company)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // GET: Projects/Create
        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public IActionResult Create()
        {
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id");
            Project? project = new();
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
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                //instantiate BTuser and get from userManager
                //assign project CompanyId from CompanyId of BTUser user
                BTUser? user = await _userManager.GetUserAsync(User); 
                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //if not valid, send to view with info below (basically copy the create method) sending back same model to correct if not valid    
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
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
            BTUser? user = await _userManager.GetUserAsync(User);

            Project? project = await _context.Projects
                .Where(p => p.CompanyId == user.CompanyId && p.Archived == false)
                .FirstOrDefaultAsync(p=>p.Id == id);



            if (project == null)
            {
                return NotFound();
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
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
                    //images
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    //dates
                    project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    _context.Update(project);
                    await _context.SaveChangesAsync();
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
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
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

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);         

            if (project == null)
            {
                return NotFound();
            }
        
            await _context.SaveChangesAsync();

            return View(project);
        }


        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.Archived = true;
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
