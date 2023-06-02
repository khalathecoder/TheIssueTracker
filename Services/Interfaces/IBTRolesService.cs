﻿using Microsoft.AspNetCore.Identity;
using TheIssueTracker.Models;

namespace TheIssueTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        
            Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
            Task<List<IdentityRole>> GetRolesAsync();
            Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
            Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
            Task<bool> IsUserInRole(BTUser member, string roleName);
            Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
            Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames);
        
    }
}
