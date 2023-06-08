using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheIssueTracker.Data;
using TheIssueTracker.Extensions;
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Controllers
{
    [Authorize(Roles = nameof(BTRoles.Admin))]
    public class InvitesController : Controller
	{
		private readonly IBTInviteService _inviteService;
		private readonly IBTProjectService _projectService;
		private readonly IBTCompanyService _companyService;
		private readonly IEmailSender _emailSender;
		private readonly UserManager<BTUser> _userManager;
		private readonly IDataProtector _protector;
		private readonly string _protectorPurpose;

		public InvitesController(IBTInviteService inviteService,
								 IBTProjectService projectService,
								 IBTCompanyService companyService,
								 IEmailSender emailSender,
								 UserManager<BTUser> userManager,
								 IDataProtectionProvider protectionProvider)
		{
			_inviteService = inviteService;
			_projectService = projectService;
			_companyService = companyService;
			_emailSender = emailSender;
			_userManager = userManager;

			_protectorPurpose = "iScribe2023!!!";
			_protector = protectionProvider.CreateProtector(_protectorPurpose);
		}


		// GET: Invites/Create	
        public async Task<IActionResult> Create()
		{
			List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

			ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");

			return View();
		}

		// POST: Invites/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]       
        public async Task<IActionResult> Create([Bind("ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
		{
			int companyId = User.Identity!.GetCompanyId();

			ModelState.Remove("InvitorId");
			if (ModelState.IsValid)
			{
				try
				{
					//assign invite values as we dont want user to be setting these values
					Guid guid = Guid.NewGuid();

					invite.CompanyToken = guid;
					invite.CompanyId = companyId;
					invite.InviteDate = DateTime.UtcNow;
					invite.InvitorId = _userManager.GetUserId(User);
					invite.IsValid = true;

					//save it
					await _inviteService.AddNewInviteAsync(invite);

					//encrypting our top secret invite info so we cannot just guess the url of the invite.
					//token is unique and hard to guess
					string token = _protector.Protect(guid.ToString());
					string email = _protector.Protect(invite.InviteeEmail!);
					string company = _protector.Protect(companyId.ToString());

					//magic url
					string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, Request.Scheme);

					//send the invite email
					string body = $@"<h4> You've been invited to join the bug tracker!</h4> <br/>
                                        {invite.Message}<br/><br/> 
                                        <a href=""{callbackUrl}"">Click here</a> to join our team.";

					string subject = $@"You've been invited to join The Issue Tracker";

					await _emailSender.SendEmailAsync(invite.InviteeEmail!, subject, body);

					return RedirectToAction("Index", "Home", new { SwalMessage = "Invite Sent!" });
				}
				catch (Exception)

				{

					throw;
				}
			}

			//if not valid, return the invite view and repop the form from view page
			List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
			ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");
			return View(invite);
		}

        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string? token, string? email, string? company)
		{
			//if any of the parameters are null return not found
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
			{
				return NotFound();
			}

			//we need to decrypt these previously encrypted values
			Guid companyToken = Guid.Parse(_protector.Unprotect(token));
			string inviteeEmail = _protector.Unprotect(email);
			int companyId = int.Parse(_protector.Unprotect(company));

			try
			{
				//use getinviteasync overloaded method with diff parameters
				Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

				if (invite is null)
				{
					return NotFound();
				}

				return View(invite);
			}
			catch (Exception)
			{

				throw;
			}

			
		}
	}
}
