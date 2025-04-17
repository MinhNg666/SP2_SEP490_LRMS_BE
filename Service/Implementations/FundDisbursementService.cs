using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implementations;
public class FundDisbursementService : IFundDisbursementService
{
    private readonly IFundDisbursementRepository _fundDisbursementRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IS3Service _s3Service;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;
    
    public FundDisbursementService(
        IFundDisbursementRepository fundDisbursementRepository,
        IProjectRepository projectRepository,
        IS3Service s3Service,
        IMapper mapper,
        LRMSDbContext context)
    {
        _fundDisbursementRepository = fundDisbursementRepository;
        _projectRepository = projectRepository;
        _s3Service = s3Service;
        _mapper = mapper;
        _context = context;
    }
    
    public async Task<int> CreateFundDisbursementRequest(CreateFundDisbursementRequest request, int userId)
    {
        try
        {
            // Check if the project exists
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");
                
            // Check if project is approved
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Cannot request funds for projects that are not approved");
                
            bool isAuthorized = false;
            int authorId = 0;
            int supervisorId = 0;
            
            // Different authorization logic based on project type
            if (project.ProjectType == (int)ProjectTypeEnum.Research)
            {
                // For Research projects - check if user is a group member with appropriate role
                if (project.GroupId == null)
                    throw new ServiceException("Research project must have an associated group");
                    
                var groupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.GroupId == project.GroupId && 
                                         gm.UserId == userId &&
                                         gm.Status == (int)GroupMemberStatus.Active);
                    
                if (groupMember != null)
                {
                    isAuthorized = true;
                    
                    // Find an existing author for the project (any author) to use for AuthorRequest
                    var projectAuthor = await _context.Authors
                        .FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId);
                        
                    if (projectAuthor == null)
                    {
                        // Create a placeholder author if none exists
                        projectAuthor = new Author
                        {
                            ProjectId = request.ProjectId,
                            UserId = userId,
                            Role = 0 // Main author or appropriate role
                        };
                        
                        _context.Authors.Add(projectAuthor);
                        await _context.SaveChangesAsync();
                    }
                    
                    authorId = projectAuthor.AuthorId;
                    
                    // Find a supervisor for the project
                    var supervisor = await _context.GroupMembers
                        .FirstOrDefaultAsync(gm => gm.GroupId == project.GroupId && 
                                            (gm.Role == (int)GroupMemberRoleEnum.Leader || 
                                             gm.Role == (int)GroupMemberRoleEnum.Supervisor) &&
                                             gm.Status == (int)GroupMemberStatus.Active);
                                             
                    if (supervisor == null)
                        throw new ServiceException("Research project must have a supervisor or leader");
                        
                    supervisorId = supervisor.GroupMemberId;
                }
            }
            else if (project.ProjectType == (int)ProjectTypeEnum.Conference || 
                     project.ProjectType == (int)ProjectTypeEnum.Journal)
            {
                // For Paper/Conference projects - check if user is an author
                var author = await _context.Authors
                    .FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId && a.UserId == userId);
                    
                if (author != null)
                {
                    isAuthorized = true;
                    authorId = author.AuthorId;
                    
                    // For conference/journal papers, find a supervisor or use another author as supervisor
                    // This is a placeholder - adjust according to your business rules
                    var otherAuthor = await _context.Authors
                        .FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId && a.AuthorId != authorId);
                        
                    if (otherAuthor != null && otherAuthor.UserId.HasValue)
                    {
                        // Find this user's group membership to use as supervisor
                        var authorAsSupervisor = await _context.GroupMembers
                            .FirstOrDefaultAsync(gm => gm.UserId == otherAuthor.UserId);
                            
                        if (authorAsSupervisor != null)
                        {
                            supervisorId = authorAsSupervisor.GroupMemberId;
                        }
                        else
                        {
                            // If no group membership found, create one for this purpose
                            throw new ServiceException("No suitable supervisor found for fund request");
                        }
                    }
                    else
                    {
                        throw new ServiceException("Papers must have at least two authors for fund requests");
                    }
                }
            }
            
            if (!isAuthorized)
                throw new ServiceException("You are not authorized to request funds for this project");
                
            // Continue with the rest of the validation...
            // Check active quotas for this project
            var availableQuota = await _context.Quotas
                .Where(q => q.ProjectId == request.ProjectId && q.Status == (int)QuotaStatusEnum.Active)
                .FirstOrDefaultAsync();
                
            if (availableQuota == null)
                throw new ServiceException("No active quota available for this project");
                
            // Check if requested amount exceeds available budget
            if (request.FundRequest > availableQuota.AllocatedBudget)
                throw new ServiceException($"Requested amount exceeds available budget. Available: {availableQuota.AllocatedBudget}");
                
            // Verify the project phase if specified
            if (request.ProjectPhaseId.HasValue)
            {
                var projectPhase = await _context.ProjectPhases
                    .FirstOrDefaultAsync(p => p.ProjectPhaseId == request.ProjectPhaseId.Value && 
                                         p.ProjectId == request.ProjectId);
                                         
                if (projectPhase == null)
                    throw new ServiceException("Specified project phase does not belong to this project");
                
                // Check if the project phase is completed
                if (projectPhase.Status == (int)ProjectPhaseStatusEnum.Completed)
                {
                    // Check if this completed phase already has a fund disbursement
                    var existingDisbursement = await _context.FundDisbursements
                        .AnyAsync(f => f.ProjectPhaseId == request.ProjectPhaseId.Value);
                        
                    if (existingDisbursement)
                    {
                        throw new ServiceException("This completed project phase already has a fund disbursement request. Only one request is allowed per completed phase.");
                    }
                }
            }
            
            // Check if there is an active fund request timeline
            var currentDate = DateTime.Now;
            var activeFundRequestTimeline = await _context.Timelines
                .Where(t => t.TimelineType == (int)TimelineTypeEnum.FundRequest &&
                       t.StartDate <= currentDate &&
                       t.EndDate >= currentDate)
                .FirstOrDefaultAsync();
                
            if (activeFundRequestTimeline == null)
                throw new ServiceException("Fund requests are not currently open. Please check the funding schedule.");
                
            // Create the fund disbursement request
            var fundDisbursement = new FundDisbursement
            {
                FundRequest = request.FundRequest,
                Description = request.Description,
                ProjectId = request.ProjectId,
                ProjectPhaseId = request.ProjectPhaseId,
                QuotaId = availableQuota.QuotaId,
                Status = (int)FundDisbursementStatusEnum.Pending,
                CreatedAt = DateTime.Now,
                AuthorRequest = authorId,
                SupervisorRequest = supervisorId
            };
            
            await _fundDisbursementRepository.AddAsync(fundDisbursement);
            
            // Update the project's spent budget
            project.SpentBudget += fundDisbursement.FundRequest ?? 0;
            project.UpdatedAt = DateTime.Now;

            // Update the project phase's spent budget
            if (fundDisbursement.ProjectPhaseId.HasValue)
            {
                var projectPhase = await _context.ProjectPhases.FindAsync(fundDisbursement.ProjectPhaseId.Value);
                if (projectPhase != null)
                {
                    projectPhase.SpentBudget += fundDisbursement.FundRequest ?? 0;
                    _context.ProjectPhases.Update(projectPhase);
                }
            }
            
            return fundDisbursement.FundDisbursementId;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating fund disbursement request: {ex.Message}");
        }
    }
    
    public async Task<IEnumerable<FundDisbursementResponse>> GetAllFundDisbursements()
    {
        try
        {
            var disbursements = await _fundDisbursementRepository.GetAllWithDetailsAsync();
            return MapToDisbursementResponses(disbursements);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving fund disbursements: {ex.Message}");
        }
    }
    
    public async Task<FundDisbursementResponse> GetFundDisbursementById(int fundDisbursementId)
    {
        try
        {
            var disbursement = await _fundDisbursementRepository.GetByIdWithDetailsAsync(fundDisbursementId);
            if (disbursement == null)
                throw new ServiceException("Fund disbursement not found");
                
            return MapToDisbursementResponse(disbursement);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving fund disbursement: {ex.Message}");
        }
    }
    
    public async Task<IEnumerable<FundDisbursementResponse>> GetFundDisbursementsByProjectId(int projectId)
    {
        try
        {
            var disbursements = await _fundDisbursementRepository.GetByProjectIdAsync(projectId);
            return MapToDisbursementResponses(disbursements);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving fund disbursements: {ex.Message}");
        }
    }
    
    public async Task<IEnumerable<FundDisbursementResponse>> GetFundDisbursementsByUserId(int userId)
    {
        try
        {
            var disbursements = await _fundDisbursementRepository.GetByUserIdAsync(userId);
            return MapToDisbursementResponses(disbursements);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error retrieving fund disbursements: {ex.Message}");
        }
    }
    
    public async Task<bool> UploadDisbursementDocuments(int fundDisbursementId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var disbursement = await _fundDisbursementRepository.GetByIdWithDetailsAsync(fundDisbursementId);
            if (disbursement == null)
                throw new ServiceException("Fund disbursement not found");
                
            // Check if user is authorized (author or supervisor)
            var author = disbursement.AuthorRequestNavigation;
            var supervisor = disbursement.SupervisorRequestNavigation;
            
            if (author?.UserId != userId && supervisor?.UserId != userId)
                throw new ServiceException("You are not authorized to upload documents for this disbursement");
                
            // Upload documents to S3
            var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{disbursement.ProjectId}/disbursements/{fundDisbursementId}");
            int index = 0;
            
            foreach (var file in documentFiles)
            {
                // Create resource
                var projectResource = new ProjectResource
                {
                    ResourceName = file.FileName,
                    ResourceType = 1, // Document
                    ProjectId = disbursement.ProjectId,
                    Acquired = true,
                    Quantity = 1
                };
                
                await _context.ProjectResources.AddAsync(projectResource);
                await _context.SaveChangesAsync();
                
                // Create document
                var document = new Document
                {
                    FundDisbursementId = fundDisbursementId,
                    DocumentUrl = urls[index],
                    FileName = file.FileName,
                    DocumentType = (int)DocumentTypeEnum.Disbursement,
                    UploadAt = DateTime.Now,
                    UploadedBy = userId,
                    ProjectResourceId = projectResource.ProjectResourceId,
                    ProjectId = disbursement.ProjectId
                };
                
                await _context.Documents.AddAsync(document);
                index++;
            }
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading disbursement documents: {ex.Message}");
        }
    }
    
    public async Task<bool> ApproveFundDisbursement(int fundDisbursementId, string approvalComments, int approverId)
    {
        try
        {
            // Use a transaction to ensure all database changes succeed or fail together
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Get the fund disbursement with details
            var fundDisbursement = await _fundDisbursementRepository.GetByIdWithDetailsAsync(fundDisbursementId);
            if (fundDisbursement == null)
                throw new ServiceException("Fund disbursement request not found");
                
            // Check if it's already approved or rejected
            if (fundDisbursement.Status == (int)FundDisbursementStatusEnum.Approved)
                throw new ServiceException("This fund disbursement request has already been approved");
                
            if (fundDisbursement.Status == (int)FundDisbursementStatusEnum.Rejected)
                throw new ServiceException("This fund disbursement request has already been rejected");
                
            // Verify the user is an Office user (this check will happen in the controller)
            var officeUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == approverId);
                
            if (officeUser == null)
                throw new ServiceException("Approver not found");
                
            if (fundDisbursement.ProjectId == null)
                throw new ServiceException("Fund disbursement is not associated with a project");
                
            // Get the project
            var project = await _projectRepository.GetByIdAsync(fundDisbursement.ProjectId.Value);
            if (project == null)
                throw new ServiceException("Project not found");
                
            // Get the quota for this disbursement
            var quota = await _context.Quotas
                .FirstOrDefaultAsync(q => q.QuotaId == fundDisbursement.QuotaId);
                
            if (quota == null)
                throw new ServiceException("Quota not found for this fund disbursement");
                
            // Check if quota has sufficient funds
            if (quota.AllocatedBudget < fundDisbursement.FundRequest)
                throw new ServiceException("Insufficient quota budget for this disbursement");
                
            // Find or create a group member for the office user
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.UserId == approverId);
                
            if (groupMember == null)
            {
                // Create a temporary group member entry for the office user
                var defaultGroup = await _context.Groups.FirstOrDefaultAsync();
                if (defaultGroup == null)
                    throw new ServiceException("No groups available to associate with approver");
                    
                groupMember = new GroupMember
                {
                    UserId = approverId,
                    GroupId = defaultGroup.GroupId,
                    Role = (int)GroupMemberRoleEnum.Member,
                    Status = (int)GroupMemberStatus.Active,
                    JoinDate = DateTime.Now
                };
                
                _context.GroupMembers.Add(groupMember);
                await _context.SaveChangesAsync();
            }
            
            // 1. Update the fund disbursement
            fundDisbursement.Status = (int)FundDisbursementStatusEnum.Approved;
            fundDisbursement.AppovedBy = groupMember.GroupMemberId;
            fundDisbursement.UpdateAt = DateTime.Now;
            
            // 2. Update the quota's remaining budget
            quota.AllocatedBudget -= fundDisbursement.FundRequest;
            quota.UpdateAt = DateTime.Now;
            
            // If quota is fully used, update its status
            if (quota.AllocatedBudget <= 0)
            {
                quota.Status = (int)QuotaStatusEnum.Used;
            }
            
            // 3. Update the project's spent budget
            project.SpentBudget += fundDisbursement.FundRequest ?? 0;
            project.UpdatedAt = DateTime.Now;

            // Update the project phase's spent budget
            if (fundDisbursement.ProjectPhaseId.HasValue)
            {
                var projectPhase = await _context.ProjectPhases.FindAsync(fundDisbursement.ProjectPhaseId.Value);
                if (projectPhase != null)
                {
                    projectPhase.SpentBudget += fundDisbursement.FundRequest ?? 0;
                    _context.ProjectPhases.Update(projectPhase);
                }
            }
            
            // 4. Optional: Create a financial transaction record if you have such a table
            // var transaction = new FinancialTransaction
            // {
            //     ProjectId = project.ProjectId,
            //     Amount = fundDisbursement.FundRequest,
            //     TransactionType = "Disbursement",
            //     TransactionDate = DateTime.Now,
            //     ApprovedBy = approverId,
            //     Description = $"Fund disbursement #{fundDisbursement.FundDisbursementId}: {fundDisbursement.Description}"
            // };
            // _context.FinancialTransactions.Add(transaction);
            
            // Save all changes
            await _fundDisbursementRepository.UpdateAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Commit the transaction
            await transaction.CommitAsync();
            
            // Create notification for requester
            if (fundDisbursement.AuthorRequestNavigation?.UserId != null)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = fundDisbursement.AuthorRequestNavigation.UserId.Value,
                    Title = "Fund Disbursement Approved",
                    Message = $"Your fund disbursement request of {fundDisbursement.FundRequest:C} has been approved. {approvalComments}",
                    ProjectId = fundDisbursement.ProjectId,
                    Status = 0,
                    IsRead = false
                };
                
                // If you have a notification service
                // await _notificationService.CreateNotification(notificationRequest);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error approving fund disbursement: {ex.Message}", ex);
        }
    }
    
    public async Task<bool> RejectFundDisbursement(int fundDisbursementId, string rejectionReason, int rejectorId)
    {
        try
        {
            // Get the fund disbursement with details
            var fundDisbursement = await _fundDisbursementRepository.GetByIdWithDetailsAsync(fundDisbursementId);
            if (fundDisbursement == null)
                throw new ServiceException("Fund disbursement request not found");
                
            // Check if it's already approved or rejected
            if (fundDisbursement.Status == (int)FundDisbursementStatusEnum.Approved)
                throw new ServiceException("This fund disbursement request has already been approved");
                
            if (fundDisbursement.Status == (int)FundDisbursementStatusEnum.Rejected)
                throw new ServiceException("This fund disbursement request has already been rejected");
                
            // Verify the user is an Office user (this check will happen in the controller)
            var officeUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == rejectorId);
                
            if (officeUser == null)
                throw new ServiceException("Rejector not found");
                
            // Find a suitable group member to use as approver (the office user might not be in any group)
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.UserId == rejectorId);
                
            if (groupMember == null)
            {
                // Create a temporary group member entry for the office user
                // This is to satisfy the database FK constraint
                var defaultGroup = await _context.Groups.FirstOrDefaultAsync();
                if (defaultGroup == null)
                    throw new ServiceException("No groups available to associate with rejector");
                    
                groupMember = new GroupMember
                {
                    UserId = rejectorId,
                    GroupId = defaultGroup.GroupId,
                    Role = (int)GroupMemberRoleEnum.Member,
                    Status = (int)GroupMemberStatus.Active,
                    JoinDate = DateTime.Now
                };
                
                _context.GroupMembers.Add(groupMember);
                await _context.SaveChangesAsync();
            }
            
            // Update fund disbursement
            fundDisbursement.Status = (int)FundDisbursementStatusEnum.Rejected;
            fundDisbursement.AppovedBy = groupMember.GroupMemberId; // Used for both approval and rejection
            fundDisbursement.UpdateAt = DateTime.Now;
            
            await _fundDisbursementRepository.UpdateAsync(fundDisbursement);
            
            // Create notification for requester
            if (fundDisbursement.AuthorRequestNavigation?.UserId != null)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = fundDisbursement.AuthorRequestNavigation.UserId.Value,
                    Title = "Fund Disbursement Rejected",
                    Message = $"Your fund disbursement request of {fundDisbursement.FundRequest:C} has been rejected. Reason: {rejectionReason}",
                    ProjectId = fundDisbursement.ProjectId,
                    Status = 0,
                    IsRead = false
                };
                
                // You'll need to inject INotificationService if you want to use this
                // await _notificationService.CreateNotification(notificationRequest);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error rejecting fund disbursement: {ex.Message}");
        }
    }
    
    public async Task<bool> UploadDisbursementDocument(int fundDisbursementId, IFormFile documentFile, int userId)
    {
        try
        {
            var disbursement = await _fundDisbursementRepository.GetByIdWithDetailsAsync(fundDisbursementId);
            if (disbursement == null)
                throw new ServiceException("Fund disbursement not found");
                
            // Check if user is authorized (author or supervisor)
            var author = disbursement.AuthorRequestNavigation;
            var supervisor = disbursement.SupervisorRequestNavigation;
            
            if (author?.UserId != userId && supervisor?.UserId != userId)
                throw new ServiceException("You are not authorized to upload documents for this disbursement");
                
            // Upload document to S3
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{disbursement.ProjectId}/disbursements/{fundDisbursementId}");
            
            // Create resource
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = disbursement.ProjectId,
                Acquired = true,
                Quantity = 1
            };
            
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();
            
            // Create document
            var document = new Document
            {
                FundDisbursementId = fundDisbursementId,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.Disbursement,
                UploadAt = DateTime.Now,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId,
                ProjectId = disbursement.ProjectId
            };
            
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading disbursement document: {ex.Message}");
        }
    }
    
    private FundDisbursementResponse MapToDisbursementResponse(FundDisbursement disbursement)
    {
        var response = new FundDisbursementResponse
        {
            FundDisbursementId = disbursement.FundDisbursementId,
            FundRequest = disbursement.FundRequest ?? 0,
            Status = disbursement.Status ?? 0,
            StatusName = Enum.GetName(typeof(FundDisbursementStatusEnum), disbursement.Status ?? 0),
            CreatedAt = disbursement.CreatedAt,
            Description = disbursement.Description,
            ProjectId = disbursement.ProjectId ?? 0,
            ProjectName = disbursement.Project?.ProjectName,
            QuotaId = disbursement.QuotaId,
            ProjectPhaseId = disbursement.ProjectPhaseId,
            ProjectPhaseTitle = disbursement.ProjectPhase?.Title,
            AuthorRequest = disbursement.AuthorRequest,
            AuthorName = disbursement.AuthorRequestNavigation?.User?.FullName,
            SupervisorRequest = disbursement.SupervisorRequest,
            SupervisorName = disbursement.SupervisorRequestNavigation?.User?.FullName,
            
            // Map documents if they exist
            Documents = disbursement.Documents != null
                ? disbursement.Documents.Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName ?? "Unknown",
                    DocumentUrl = d.DocumentUrl ?? "",
                    DocumentType = d.DocumentType ?? 0,
                    UploadAt = d.UploadAt ?? DateTime.MinValue
                }).ToList()
                : new List<DocumentResponse>()
        };
        
        return response;
    }
    
    private IEnumerable<FundDisbursementResponse> MapToDisbursementResponses(IEnumerable<FundDisbursement> disbursements)
    {
        return disbursements.Select(d => MapToDisbursementResponse(d));
    }
}
