using AutoMapper;
using Domain.Constants;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class QuotaService : IQuotaService
    {
        private readonly LRMSDbContext _context;
        private readonly IMapper _mapper;

        public QuotaService(LRMSDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuotaResponse>> GetAllQuotas()
        {
            try
            {
                var quotas = await _context.Quotas
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Department)
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Group)
                    .Include(q => q.AllocatedByNavigation)
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in quotas)
                {
                    var response = new QuotaResponse
                    {
                        QuotaId = quota.QuotaId,
                        AllocatedBudget = quota.AllocatedBudget,
                        Status = quota.Status,
                        CreatedAt = quota.CreatedAt,
                        UpdateAt = quota.UpdateAt,
                        ProjectId = quota.ProjectId,
                        ProjectName = quota.Project?.ProjectName,
                        
                        // Additional project information
                        ProjectApprovedBudget = quota.Project?.ApprovedBudget,
                        ProjectSpentBudget = quota.Project?.SpentBudget ?? 0,
                        
                        // Department information
                        DepartmentId = quota.Project?.DepartmentId,
                        DepartmentName = quota.Project?.Department?.DepartmentName,
                        
                        // Group information
                        GroupId = quota.Project?.GroupId,
                        GroupName = quota.Project?.Group?.GroupName,
                        
                        // Allocator information
                        AllocatedBy = quota.AllocatedBy,
                        AllocatorName = quota.AllocatedByNavigation?.FullName
                    };
                    
                    responses.Add(response);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting all quotas: {ex.Message}");
            }
        }

        public async Task<IEnumerable<QuotaResponse>> GetQuotasByUserId(int userId)
        {
            try
            {
                // Get all groups the user is a member of
                var userGroups = await _context.GroupMembers
                    .Where(gm => gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                if (!userGroups.Any())
                {
                    return Enumerable.Empty<QuotaResponse>();
                }

                // Get projects associated with these groups
                var projectIds = await _context.Projects
                    .Where(p => p.GroupId.HasValue && userGroups.Contains(p.GroupId.Value))
                    .Select(p => p.ProjectId)
                    .ToListAsync();

                if (!projectIds.Any())
                {
                    return Enumerable.Empty<QuotaResponse>();
                }

                // Get quotas for these projects
                var quotas = await _context.Quotas
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Department)
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Group)
                    .Include(q => q.AllocatedByNavigation)
                    .Where(q => q.ProjectId.HasValue && projectIds.Contains(q.ProjectId.Value))
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in quotas)
                {
                    var response = new QuotaResponse
                    {
                        QuotaId = quota.QuotaId,
                        AllocatedBudget = quota.AllocatedBudget,
                        Status = quota.Status,
                        CreatedAt = quota.CreatedAt,
                        UpdateAt = quota.UpdateAt,
                        ProjectId = quota.ProjectId,
                        ProjectName = quota.Project?.ProjectName,
                        
                        // Additional project information
                        ProjectApprovedBudget = quota.Project?.ApprovedBudget,
                        ProjectSpentBudget = quota.Project?.SpentBudget ?? 0,
                        
                        // Department information
                        DepartmentId = quota.Project?.DepartmentId,
                        DepartmentName = quota.Project?.Department?.DepartmentName,
                        
                        // Group information
                        GroupId = quota.Project?.GroupId,
                        GroupName = quota.Project?.Group?.GroupName,
                        
                        // Allocator information
                        AllocatedBy = quota.AllocatedBy,
                        AllocatorName = quota.AllocatedByNavigation?.FullName
                    };
                    
                    responses.Add(response);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting quotas by user ID: {ex.Message}");
            }
        }
    }
}
