﻿using TheIssueTracker.Models;

namespace TheIssueTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        Task AddTicketAsync(Ticket ticket);
        Task ArchiveTicketAsync(Ticket ticket, int companyId);
        Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);
        Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId);
        Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId);
        Task<List<TicketStatus>> GetTicketStatuses();
        Task<List<TicketType>> GetTicketTypes();
        Task<List<TicketPriority>> GetTicketPriorities();
        Task RestoreTicketAsync(Ticket ticket, int companyId);
        Task UpdateTicketAsync(Ticket ticket, int companyId);
    }
}
