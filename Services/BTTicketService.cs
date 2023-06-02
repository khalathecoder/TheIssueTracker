using System.ComponentModel.Design;
using Microsoft.EntityFrameworkCore;
using TheIssueTracker.Data;
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context; //inject db
        private readonly IBTRolesService _rolesService;
       
        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
            
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket != null)
                {
                    ticket.Archived = true;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = await _context.Tickets
                                                .Where(t => t.Archived == true && t.Project!.CompanyId == companyId)
                                                .Include(t => t.Project)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.TicketPriority)
                                                .ToListAsync();

            return tickets;
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .Include(t => t.TicketComments)
                                               .Include(t => t.TicketAttachments)
                                               .Include(t => t.TicketHistories)
                                               .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            try
            {
                List<TicketPriority> ticketPriorities = await _context.TicketPriorities.ToListAsync();

                return ticketPriorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                       .Where(t => t.Archived == false && t.Project!.CompanyId == companyId)
                                                       .Include(t => t.DeveloperUser)
                                                       .Include(t => t.Project)
                                                       .Include(t => t.SubmitterUser)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)
                                                       .Include(t => t.TicketType)
                                                       .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            try
            {
                List<TicketStatus> ticketStatuses = await _context.TicketStatuses.ToListAsync();

                return ticketStatuses;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            try
            {
                List<TicketType> ticketTypes = await _context.TicketTypes.ToListAsync();

                return ticketTypes;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket != null)
                {
                    ticket.Archived = false;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket?>> GetTicketByUserIdAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);
                if (user is null) return new List<Ticket?>();

                //admin -> tickets that belong to their projects
                if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    return await GetTicketsByCompanyIdAsync(user.CompanyId);
                }
                //PM --> tickets that below to their projects
                else if (await _rolesService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                {
                    return await _context.Tickets
                                        .Include(t => t.DeveloperUser)
                                        .Include(t => t.SubmitterUser)
                                        .Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType)
                                        .Include(t => t.Project)
                                            .ThenInclude(p => p!.Members)
                                        .Where(t => !t.Archived && t.Project!.Members.Any(m => m.Id == userId))
                                        .ToListAsync();
                }
                else
                {
                    //submitter -> tickets they have submitted
                    //developer -> tickets they have been assigned or submitted
                    return await _context.Tickets
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.TicketPriority)
                                            .Include(t => t.TicketStatus)
                                            .Include(t => t.TicketType)
                                            .Include(t => t.Project)
                                                .ThenInclude(p => p!.Members)
                                            .Where(t => !t.Archived && (t.DeveloperUserId == userId || t.SubmitterUserId == userId))
                                            .ToListAsync();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets.Where(t => t.DeveloperUserId == null && t.Archived == false && t.Project!.CompanyId == companyId)
                    .Include(t => t.Project)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.SubmitterUser)
                    .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                                 .Include(t => t.Project)
                                                    .ThenInclude(p => p!.Company)
                                                .Include(t => t.TicketAttachments)
                                                .Include(t => t.TicketComments)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.TicketHistories)
                                                .Include(t => t.SubmitterUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);

                return ticket!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetDeveloperAsync(int projectId, int companyId)
        {
            try
            {
                //get project first
                Project? project = await _context.Projects
                                                .AsNoTracking()
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);


                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)))
                        {
                            return member;
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
