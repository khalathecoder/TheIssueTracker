using TheIssueTracker.Models;
using TheIssueTracker.Services.Interfaces;

namespace TheIssueTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveProjectAsync(Project project, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task RestoreProjectAsync(Project project, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProjectAsync(Project project, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}
