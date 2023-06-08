using Microsoft.EntityFrameworkCore;
using TheIssueTracker.Data;
using TheIssueTracker.Models;
using TheIssueTracker.Services.Interfaces;


namespace TheIssueTracker.Services
{
	public class BTCompanyService : IBTCompanyService
	{
		private readonly ApplicationDbContext _context;

		public BTCompanyService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Company?> GetCompanyInfoAsync(int companyId)
		{
			try
			{
				Company? company = await _context.Companies
												 .Include(c => c.Members)
												 .Include(c => c.Projects)
													.ThenInclude(p => p.Tickets)
												 .Include(c => c.Invites)
												 .FirstOrDefaultAsync(c => c.Id == companyId);

				return company;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<List<BTUser>> GetCompanyMembersAsync(int companyId)
		{
			try
			{
				List<BTUser> users = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
				return users;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
