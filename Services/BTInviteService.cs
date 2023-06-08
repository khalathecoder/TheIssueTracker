using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using TheIssueTracker.Data;
using TheIssueTracker.Models;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
	public class BTInviteService : IBTInviteService
	{
		private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
		{
			try
			{
				//get invite first
				Invite? invite = await _context.Invites
											   .FirstOrDefaultAsync(i => i.CompanyToken == token 
																	&& i.CompanyId == companyId
																	&& i.IsValid == true);

				//if null, will not accept
				if (invite == null)
				{
					return false;
				}

				invite.IsValid = false;
				invite.InviteeId = userId;
				await _context.SaveChangesAsync();

				return true;
			}
			catch (Exception)
			{
				return false;
				throw;
			}
		}

		public async Task AddNewInviteAsync(Invite invite)
		{
			try
			{
				_context.Add(invite);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
		{
			try
			{
				bool result = await _context.Invites
										.Where(i => i.CompanyId == companyId)
										.AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

				return result;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task CancelInviteAsync(int inviteId, int companyId)
		{
			try
			{
				Invite? invite = await _context.Invites
											   .FirstOrDefaultAsync(i => i.Id == inviteId
																	&& i.CompanyId == companyId);

				if (invite is not null)
				{
					invite.IsValid = false;

					await _context.SaveChangesAsync();
				}
				
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<Invite?> GetInviteAsync(int inviteId, int companyId)
		{
			try
			{
				Invite? invite = await _context.Invites
												.Include(i=>i.Company)
												.Include(i=>i.Invitor)
												.Include(i=>i.Invitee)
												.Include(i=>i.Project)
												.FirstOrDefaultAsync(i => i.Id == inviteId && i.CompanyId == companyId);

				return invite;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<Invite?> GetInviteAsync(Guid token, string email, int companyId)
		{
			try
			{
				Invite? invite = await _context.Invites
												.Include(i => i.Company)
												.Include(i => i.Invitor)
												.Include(i => i.Invitee)
												.Include(i => i.Project)
												.FirstOrDefaultAsync(i => i.CompanyToken == token 
																			&& i.CompanyId == companyId 
																			&& i.InviteeEmail == email);

				return invite;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task UpdateInviteAsync(Invite invite)
		{
			try
			{
				_context.Update(invite);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> ValidateInviteCodeAsync(Guid? token)
		{
			try
			{
				if (token is null) return false;

				//by default not a valid invite
				bool result = false;

				//is there an invite with this token
				Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

				if (invite is not null)
				{
					DateTime inviteDate = invite.InviteDate;

					//is date less than 7 days from invite date
					bool notExpired = (DateTime.UtcNow - inviteDate).TotalDays <= 7;

					if (notExpired)
					{
						//is result still valid
						result = invite.IsValid;
					}
				}
				return result;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
