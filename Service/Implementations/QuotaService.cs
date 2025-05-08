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
                    .Include(q => q.FundDisbursements)
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in quotas)
                {
                    decimal disbursedAmount = quota.FundDisbursements
                        .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                                    fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                        .Sum(fd => fd.FundRequest ?? 0);
                    
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
                        AllocatorName = quota.AllocatedByNavigation?.FullName,

                        ProjectType = quota.Project?.ProjectType,
                        ProjectTypeName = quota.Project?.ProjectType.HasValue == true
                            ? Enum.GetName(typeof(ProjectTypeEnum), quota.Project.ProjectType) 
                            : null,
                            
                        DisbursedAmount = disbursedAmount
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
                    .Include(q => q.FundDisbursements)
                    .Where(q => q.ProjectId.HasValue && projectIds.Contains(q.ProjectId.Value))
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in quotas)
                {
                    decimal disbursedAmount = quota.FundDisbursements
                        .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                                    fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                        .Sum(fd => fd.FundRequest ?? 0);
                    
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
                        AllocatorName = quota.AllocatedByNavigation?.FullName,

                        ProjectType = quota.Project?.ProjectType,
                        ProjectTypeName = quota.Project?.ProjectType.HasValue == true
                            ? Enum.GetName(typeof(ProjectTypeEnum), quota.Project.ProjectType) 
                            : null,
                            
                        DisbursedAmount = disbursedAmount
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

        public async Task<QuotaDetailResponse> GetQuotaDetailById(int quotaId)
        {
            try
            {
                var quota = await _context.Quotas
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Department)
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Group)
                    .Include(q => q.AllocatedByNavigation)
                    .Include(q => q.FundDisbursements)
                        .ThenInclude(fd => fd.UserRequestNavigation)
                    .Include(q => q.FundDisbursements)
                        .ThenInclude(fd => fd.AppovedByNavigation)
                            .ThenInclude(gm => gm.User)
                    .Include(q => q.FundDisbursements)
                        .ThenInclude(fd => fd.ProjectPhase)
                    .FirstOrDefaultAsync(q => q.QuotaId == quotaId);

                if (quota == null)
                    throw new ServiceException("Quota not found");

                // Load ProjectRequests for all FundDisbursements to get approver information
                var fundDisbursementIds = quota.FundDisbursements.Select(fd => fd.FundDisbursementId).ToList();
                var projectRequests = await _context.ProjectRequests
                    .Include(pr => pr.ApprovedBy)
                    .Where(pr => pr.FundDisbursementId.HasValue && fundDisbursementIds.Contains(pr.FundDisbursementId.Value))
                    .ToDictionaryAsync(pr => pr.FundDisbursementId.Value, pr => pr);
                
                decimal disbursedAmount = quota.FundDisbursements
                    .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                                fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                    .Sum(fd => fd.FundRequest ?? 0);
                
                var response = new QuotaDetailResponse
                {
                    // Copy all basic quota properties
                    QuotaId = quota.QuotaId,
                    AllocatedBudget = quota.AllocatedBudget,
                    Status = quota.Status,
                    CreatedAt = quota.CreatedAt,
                    UpdateAt = quota.UpdateAt,
                    ProjectId = quota.ProjectId,
                    ProjectName = quota.Project?.ProjectName,
                    
                    ProjectApprovedBudget = quota.Project?.ApprovedBudget,
                    ProjectSpentBudget = quota.Project?.SpentBudget ?? 0,
                    
                    DepartmentId = quota.Project?.DepartmentId,
                    DepartmentName = quota.Project?.Department?.DepartmentName,
                    
                    GroupId = quota.Project?.GroupId,
                    GroupName = quota.Project?.Group?.GroupName,
                    
                    AllocatedBy = quota.AllocatedBy,
                    AllocatorName = quota.AllocatedByNavigation?.FullName,

                    ProjectType = quota.Project?.ProjectType,
                    ProjectTypeName = quota.Project?.ProjectType.HasValue == true
                        ? Enum.GetName(typeof(ProjectTypeEnum), quota.Project.ProjectType) 
                        : null,
                        
                    DisbursedAmount = disbursedAmount,
                    
                    // Add the detailed disbursements
                    Disbursements = quota.FundDisbursements.Select(fd => 
                    {
                        // Get the associated ProjectRequest to extract approver information
                        projectRequests.TryGetValue(fd.FundDisbursementId, out var request);
                        
                        return new DisbursementInfo
                        {
                            FundDisbursementId = fd.FundDisbursementId,
                            FundRequest = fd.FundRequest ?? 0,
                            Status = fd.Status ?? 0,
                            StatusName = Enum.GetName(typeof(FundDisbursementStatusEnum), fd.Status ?? 0),
                            CreatedAt = fd.CreatedAt,
                            Description = fd.Description,
                            
                            RequesterId = fd.UserRequest ?? 0,
                            RequesterName = fd.UserRequestNavigation?.FullName,
                            
                            // Get approver info from ProjectRequest instead of FundDisbursement.AppovedByNavigation
                            ApprovedById = request?.ApprovedById,
                            ApprovedByName = request?.ApprovedBy?.FullName,
                            
                            ProjectPhaseId = fd.ProjectPhaseId,
                            ProjectPhaseTitle = fd.ProjectPhase?.Title,
                            
                            // Add fund disbursement type information
                            FundDisbursementType = fd.FundDisbursementType,
                            FundDisbursementTypeName = fd.FundDisbursementType.HasValue ? 
                                Enum.GetName(typeof(FundDisbursementTypeEnum), fd.FundDisbursementType.Value) : null
                        };
                    }).ToList()
                };
                
                return response;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting quota details: {ex.Message}");
            }
        }
    }
}
