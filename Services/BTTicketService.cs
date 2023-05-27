using TheIssueTracker.Models;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        public Task AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketPriority>> GetTicketPriorities()
        {
            throw new NotImplementedException();
        }

        public Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketStatus>> GetTicketStatuses()
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketType>> GetTicketTypes()
        {
            throw new NotImplementedException();
        }

        public Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
