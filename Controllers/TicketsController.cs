using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Sockets;
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
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;


        public TicketsController(ApplicationDbContext context, 
                                 UserManager<BTUser> userManager,
                                 IBTProjectService projectService, 
                                 IBTTicketService ticketService, 
                                 IBTRolesService rolesService, 
                                 IBTFileService fileService, IBTTicketHistoryService ticketHistoryService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {           
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetTicketsByCompanyIdAsync(companyId);
          
            return View(tickets);
        }

        public async Task<IActionResult> IndexCopy()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());
           
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

		public async Task<IActionResult> DetailsCopy(int? id)
		{
			if (id == null || _context.Tickets == null)
			{
				return NotFound();
			}

			Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

			if (ticket == null)
			{
				return NotFound();
			}

			return View(ticket);
		}

		// GET: Tickets/Create
		[HttpGet]
        public async Task<IActionResult> Create()
        {           

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
			List<TicketPriority> priorities = await _ticketService.GetTicketPriorities();
			List<TicketType> types = await _ticketService.GetTicketTypes();
            List<TicketStatus> statuses = await _ticketService.GetTicketStatuses();

			ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(types, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(statuses, "Id", "Name");
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ProjectId,TicketTypeId,TicketPriorityId,DeveloperUserId")] Ticket ticket)
        {
         
            ModelState.Remove("SubmitterUserId"); //since submitteruserid is required, need to remove to 
            if (ModelState.IsValid)
            {


                BTUser? user = await _userManager.GetUserAsync(User); //instantiating new object of BtUser

                //search projects db where companyid matches with user.company id and where id of current project matches ticket of projectid
                Project? project = await _context.Projects
                                                 .Where(p=>p.CompanyId == user!.CompanyId && p.Archived == false)
                                                 .FirstOrDefaultAsync(p=>p.Id == ticket.ProjectId); 

                //if project is null return notfound
                if (project == null) return NotFound();

                //changed created date for postgres
                ticket.Created = DateTime.UtcNow; 

                //assign user.id to submitteruserId
                ticket.SubmitterUserId = user!.Id; 

                //instantiate object of ticketstatus and look in db of TicketStatuses.Name where the name matches the status of "new" since new tickets will always be status of new
                TicketStatus? ticketStatus = await _context.TicketStatuses
                                                           .FirstOrDefaultAsync(ts => ts.Name == nameof(BTTicketStatuses.New));

                //assign ticketstatus.id above to TicketStatusid of ticket
                ticket.TicketStatusId = ticketStatus!.Id;

                await _ticketService.AddTicketAsync(ticket);

                await _ticketHistoryService.AddHistoryAsync(null, ticket, _userManager.GetUserId(User)!);

                return RedirectToAction(nameof(Index));
            }

			List<TicketPriority> priorities = await _ticketService.GetTicketPriorities();
			List<TicketType> types = await _ticketService.GetTicketTypes();
			List<TicketStatus> statuses = await _ticketService.GetTicketStatuses();

			ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);          
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(statuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket is null)
            {
                return NotFound();
            }

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), User.Identity!.GetCompanyId());

            BTUser? currentDeveloper = await _ticketService.GetDeveloperAsync(id.Value, User.Identity!.GetCompanyId());

            AssignDeveloperViewModel viewModel = new AssignDeveloperViewModel()
            {
                Ticket = ticket,
                DeveloperId = currentDeveloper?.Id,
                DeveloperList = new SelectList(developers, "Id", "FullName", currentDeveloper?.Id)

            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel viewModel)
        {
			if (viewModel.Ticket?.Id is not null)
			{
				Ticket? ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket.Id, User.Identity!.GetCompanyId());
				if (ticket is not null)
				{
					ticket.DeveloperUserId = viewModel.DeveloperId;

					await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());

					return RedirectToAction(nameof(Details), new { id = viewModel.Ticket!.Id });
				}
			}

			return BadRequest();
		}

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.UtcNow;

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, User.Identity!.GetCompanyId());

                    await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());
                    ticket = (await _ticketService.GetTicketByIdAsync(ticket.Id, User.Identity!.GetCompanyId()))!;

                    await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, _userManager.GetUserId(User)!);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        
        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            //int companyId = User.Identity!.GetCompanyId();

            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, User.Identity!.GetCompanyId());

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket, User.Identity!.GetCompanyId());
            }

            await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());
            return RedirectToAction(nameof(Index));
        }


       public async Task<IActionResult> MyTickets()
        {
            BTUser? user = await _userManager.GetUserAsync(User);
            return View(await _ticketService.GetTicketByUserIdAsync(user!.Id));

        }

        public async Task<IActionResult> ArchivedTickets()
        {
            return View(await _ticketService.GetArchivedTicketsAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> UnassignedTickets()
        {

            return View(await _ticketService.GetUnassignedTicketsAsync(User.Identity!.GetCompanyId()));
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.Description = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                await _ticketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.User.Id);
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.Description;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,TicketId")] TicketComment ticketComment)
        {
            string statusMessage;
            ModelState.Remove("UserId");

			if (ModelState.IsValid)
			{
				ticketComment.Created = DateTime.UtcNow;
				ticketComment.UserId = _userManager.GetUserId(User);


                await _ticketService.AddTicketCommentAsync(ticketComment);

                await _ticketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                statusMessage = "Success: New comment added to Ticket.";
                //return RedirectToAction("Details", "Projects", new { ticketComment.TicketId });
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }
            return RedirectToAction("Details", new { id = ticketComment.TicketId, messsage = statusMessage});

        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
