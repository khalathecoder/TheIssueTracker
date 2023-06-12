using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TheIssueTracker.Data;
using TheIssueTracker.Models;
using TheIssueTracker.Models.Enums;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context; //inject db
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = true;

                    //archive all the tickets
                    foreach (Ticket ticket in project.Tickets)
                    {
                        //archived by project if ticket is not already archived
                        if (ticket.Archived == false) ticket.ArchivedByProject = true;

                        ticket.ArchivedByProject = true;
                    }
                }
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                        .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                        .Include(p => p.ProjectPriority)
                                                        .Include(p => p.Tickets)
                                                        .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                List<Project> projects = await _context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == false)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.DeveloperUser)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Members)
                .Where(p => string.Equals(priority, p.ProjectPriority!.Name))
                .ToListAsync();

                return projects;
            }
            catch (Exception)
            {
                return new List<Project>();
                throw;
            }
        }

        public async Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            try
            {

                List<Project> projects = await _context.Projects
                                                              .Include(p => p.Members)
                                                              .Include(p => p.ProjectPriority)
                                                              .Include(p => p.Tickets)
                                                              .Where(p => p.Members.Any(m => m.Id == userId))
                                                              .ToListAsync();

                return projects;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                        .Where(p => p.CompanyId == companyId && p.Archived == true)
                                                        .Include(p => p.ProjectPriority)
                                                        .Include(p => p.Tickets)
                                                        .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                return await _context.Projects
                                     .Include(p => p.Company)
                                     .Include(p => p.Members)
                                     .Include(p => p.ProjectPriority)
                                     .Include(p => p.Tickets)
                                        .ThenInclude(t => t.DeveloperUser)
                                     .Include(p => p.Tickets)
                                        .ThenInclude(t => t.SubmitterUser)
                                     .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                List<ProjectPriority> priorities = await _context.ProjectPriorities.ToListAsync();

                return priorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = false;

                    //archive all the tickets
                    foreach (Ticket ticket in project.Tickets)
                    {
                        //archived by project if ticket is not already archived
                        if (ticket.ArchivedByProject == true) ticket.Archived = false;

                        //either way, its definitely no longer archived by proj
                        ticket.ArchivedByProject = false;
                    }
                }
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
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
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
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

        public async Task<bool> AddProjectManagerAsync(string? userId, int projectId, int companyId)
        {
            try
            {
                //get the project for this company
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                //get user from this company
                BTUser? projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

                if (project is not null && projectManager is not null) 
                {
                    //make sure the user is a PM
                    if (!await _rolesService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager))) return false;

                    //remove any potentially existing PM
                    await RemoveProjectManagerAsync(projectId, companyId);

                    //assign the new PM
                    project.Members.Add(projectManager);

                    //save changes
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            project.Members.Remove(member); //removing from collection of members, not the _context (db)
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
        {

            try
            {
                Project? project = await _context.Projects
                                    .AsNoTracking()
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    List<BTUser> members = project.Members.ToList();
                    List<BTUser> projectMembers = new();
                    foreach (BTUser member in members)
                    {
                        if (await _rolesService.IsUserInRole(member, roleName))
                        {
                            projectMembers.Add(member);
                        }
                    }
                    return projectMembers;
                }

                return new List<BTUser>();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                //get project by Id
                Project? project = await GetProjectByIdAsync(projectId, companyId);


                if (project is not null)
                {
                    project.Members.Add(member);
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                //get project by Id
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                if (project is not null)
                {
                    project.Members.Remove(member);
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> allProjects = await GetAllProjectsByCompanyIdAsync(companyId);
                List<Project> unassignedProjects = new();

                foreach (Project project in allProjects)
                {
                    BTUser? projectManager = await GetProjectManagerAsync(project.Id, companyId);
                    if (projectManager is null) unassignedProjects.Add(project);
                }

                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
