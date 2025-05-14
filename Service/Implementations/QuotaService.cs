using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
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
                    .Include(q => q.Department)
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
                        DepartmentId = quota.DepartmentId ?? quota.Project?.DepartmentId,
                        DepartmentName = quota.Department?.DepartmentName ?? quota.Project?.Department?.DepartmentName,
                        
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
                            
                        DisbursedAmount = disbursedAmount,
                        
                        // New department quota fields
                        NumProjects = quota.NumProjects,
                        QuotaYear = quota.QuotaYear
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
                    .Include(q => q.Department)
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
                        DepartmentId = quota.DepartmentId ?? quota.Project?.DepartmentId,
                        DepartmentName = quota.Department?.DepartmentName ?? quota.Project?.Department?.DepartmentName,
                        
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
                            
                        DisbursedAmount = disbursedAmount,
                        
                        // New department quota fields
                        NumProjects = quota.NumProjects,
                        QuotaYear = quota.QuotaYear
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
                    .Include(q => q.Department)
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
                    
                    DepartmentId = quota.DepartmentId ?? quota.Project?.DepartmentId,
                    DepartmentName = quota.Department?.DepartmentName ?? quota.Project?.Department?.DepartmentName,
                    
                    GroupId = quota.Project?.GroupId,
                    GroupName = quota.Project?.Group?.GroupName,
                    
                    AllocatedBy = quota.AllocatedBy,
                    AllocatorName = quota.AllocatedByNavigation?.FullName,

                    ProjectType = quota.Project?.ProjectType,
                    ProjectTypeName = quota.Project?.ProjectType.HasValue == true
                        ? Enum.GetName(typeof(ProjectTypeEnum), quota.Project.ProjectType) 
                        : null,
                        
                    DisbursedAmount = disbursedAmount,
                    
                    // New department quota fields
                    NumProjects = quota.NumProjects,
                    QuotaYear = quota.QuotaYear,
                    
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

        public async Task<int> AllocateQuotaToDepartment(AllocateDepartmentQuotaRequest request, int allocatedBy)
        {
            try
            {
                // Validate request
                if (request.AllocatedBudget <= 0)
                    throw new ServiceException("Allocated budget must be greater than 0");

                // Check if department exists
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentId == request.DepartmentId);

                if (department == null)
                    throw new ServiceException($"Department with ID {request.DepartmentId} not found");

                // Create new department quota
                var quota = new Quota
                {
                    AllocatedBudget = request.AllocatedBudget,
                    Status = (int)QuotaStatusEnum.Active,
                    CreatedAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    DepartmentId = request.DepartmentId,
                    AllocatedBy = allocatedBy,
                    NumProjects = request.NumProjects,
                    QuotaYear = request.QuotaYear ?? DateTime.Now.Year
                };

                await _context.Quotas.AddAsync(quota);
                await _context.SaveChangesAsync();

                return quota.QuotaId;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error allocating quota to department: {ex.Message}");
            }
        }

        public async Task<IEnumerable<QuotaResponse>> GetDepartmentQuotas()
        {
            try
            {
                var departmentQuotas = await _context.Quotas
                    .Include(q => q.Department)
                    .Include(q => q.AllocatedByNavigation)
                    .Include(q => q.FundDisbursements)
                    .Where(q => q.DepartmentId.HasValue && !q.ProjectId.HasValue)
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in departmentQuotas)
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
                        ProjectId = null,
                        ProjectName = null,
                        ProjectApprovedBudget = null,
                        ProjectSpentBudget = 0,
                        
                        // Department information
                        DepartmentId = quota.DepartmentId,
                        DepartmentName = quota.Department?.DepartmentName,
                        
                        // Allocator information
                        AllocatedBy = quota.AllocatedBy,
                        AllocatorName = quota.AllocatedByNavigation?.FullName,
                        
                        DisbursedAmount = disbursedAmount,
                        
                        // Department quota fields
                        NumProjects = quota.NumProjects,
                        QuotaYear = quota.QuotaYear
                    };
                    
                    responses.Add(response);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting department quotas: {ex.Message}");
            }
        }

        public async Task<IEnumerable<QuotaResponse>> GetQuotasByDepartmentId(int departmentId)
        {
            try
            {
                var departmentQuotas = await _context.Quotas
                    .Include(q => q.Department)
                    .Include(q => q.Project)
                    .Include(q => q.AllocatedByNavigation)
                    .Include(q => q.FundDisbursements)
                    .Where(q => q.DepartmentId == departmentId)
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in departmentQuotas)
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
                        DepartmentId = quota.DepartmentId,
                        DepartmentName = quota.Department?.DepartmentName,
                        
                        // Allocator information
                        AllocatedBy = quota.AllocatedBy,
                        AllocatorName = quota.AllocatedByNavigation?.FullName,

                        ProjectType = quota.Project?.ProjectType,
                        ProjectTypeName = quota.Project?.ProjectType.HasValue == true
                            ? Enum.GetName(typeof(ProjectTypeEnum), quota.Project.ProjectType) 
                            : null,
                            
                        DisbursedAmount = disbursedAmount,
                        
                        // Department quota fields
                        NumProjects = quota.NumProjects,
                        QuotaYear = quota.QuotaYear
                    };
                    
                    responses.Add(response);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting quotas by department ID: {ex.Message}");
            }
        }

        public async Task<decimal> GetAvailableDepartmentBudget(int departmentId)
        {
            try
            {
                // Get active department quotas
                var departmentQuotas = await _context.Quotas
                    .Where(q => q.DepartmentId == departmentId && 
                           q.Status == (int)QuotaStatusEnum.Active &&
                           !q.ProjectId.HasValue)
                    .ToListAsync();
                
                if (!departmentQuotas.Any())
                    return 0;

                // Calculate the total available budget from all active quotas
                decimal totalAvailableBudget = departmentQuotas.Sum(q => q.AllocatedBudget ?? 0);
                
                return totalAvailableBudget;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting available department budget: {ex.Message}");
            }
        }

        public async Task<int> CreateProjectQuota(int projectId, decimal amount, int departmentId, int allocatedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check available department budget
                var availableBudget = await GetAvailableDepartmentBudget(departmentId);
                
                if (availableBudget < amount)
                    throw new ServiceException($"Insufficient department budget. Available: {availableBudget}, Required: {amount}");
                
                // Get the oldest active department quota to deduct from
                var departmentQuota = await _context.Quotas
                    .Where(q => q.DepartmentId == departmentId && 
                           q.Status == (int)QuotaStatusEnum.Active &&
                           !q.ProjectId.HasValue &&
                           (q.AllocatedBudget ?? 0) >= amount)
                    .OrderBy(q => q.CreatedAt)
                    .FirstOrDefaultAsync();
                
                if (departmentQuota == null)
                    throw new ServiceException("No single department quota with sufficient budget found");
                
                // Create new project quota linked to the department
                var projectQuota = new Quota
                {
                    AllocatedBudget = amount,
                    Status = (int)QuotaStatusEnum.Active,
                    CreatedAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    ProjectId = projectId,
                    DepartmentId = departmentId,
                    AllocatedBy = allocatedBy
                };
                
                await _context.Quotas.AddAsync(projectQuota);
                
                // Deduct from department quota
                departmentQuota.AllocatedBudget -= amount;
                departmentQuota.UpdateAt = DateTime.Now;
                
                // If department quota is fully used, update its status
                if (departmentQuota.AllocatedBudget <= 0)
                {
                    departmentQuota.Status = (int)QuotaStatusEnum.Used;
                }
                
                _context.Quotas.Update(departmentQuota);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                return projectQuota.QuotaId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ServiceException($"Error creating project quota: {ex.Message}");
            }
        }

        public async Task<IEnumerable<QuotaResponse>> GetDepartmentProjectQuotas(int departmentId)
        {
            try
            {
                var projectQuotas = await _context.Quotas
                    .Include(q => q.Department)
                    .Include(q => q.Project)
                        .ThenInclude(p => p.Group)
                    .Include(q => q.AllocatedByNavigation)
                    .Include(q => q.FundDisbursements)
                    .Where(q => q.DepartmentId == departmentId && q.ProjectId.HasValue)
                    .ToListAsync();

                var responses = new List<QuotaResponse>();
                
                foreach (var quota in projectQuotas)
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
                        DepartmentId = quota.DepartmentId,
                        DepartmentName = quota.Department?.DepartmentName,
                        
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
                            
                        DisbursedAmount = disbursedAmount,
                        
                        // Department quota fields
                        NumProjects = quota.NumProjects,
                        QuotaYear = quota.QuotaYear
                    };
                    
                    responses.Add(response);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting project quotas for department: {ex.Message}");
            }
        }
    }
}
