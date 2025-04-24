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
            // Use a transaction to ensure both the fund disbursement and project request are created atomically
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Check if the project exists
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");
                
            // Check if project is approved
            if (project.Status != (int)ProjectStatusEnum.Approved)
                throw new ServiceException("Cannot request funds for projects that are not approved");
                
            // Get the user making the request
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null)
                throw new ServiceException("User not found");
            
            // Check if user is a lecturer 
            bool isLecturer = false;
            if (user != null)
            {
                isLecturer = user.Role == (int)SystemRoleEnum.Lecturer;
            }
            
            // Check project permissions based on project type
            if (project.ProjectType == (int)ProjectTypeEnum.Research)
            {
                // For Research projects - check if user is a group member
                if (project.GroupId == null)
                    throw new ServiceException("Research project must have an associated group");
                    
                var userGroupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.GroupId == project.GroupId && 
                                         gm.UserId == userId &&
                                         gm.Status == (int)GroupMemberStatus.Active);
                    
                if (userGroupMember == null)
                    throw new ServiceException("You are not a member of this project's research group");
                
                // Find the supervisor for the project (just for permission check)
                var supervisor = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.GroupId == project.GroupId && 
                                         (gm.Role == (int)GroupMemberRoleEnum.Supervisor) &&
                                         gm.Status == (int)GroupMemberStatus.Active);
                                        
                if (supervisor == null)
                    throw new ServiceException("Research project must have a supervisor");
                
                // Only allow lecturers or the supervisor to create disbursement requests
                if (!isLecturer && userGroupMember.GroupMemberId != supervisor.GroupMemberId)
                    throw new ServiceException("Only supervisors or lecturers can create fund disbursement requests");
            }
            else if (project.ProjectType == (int)ProjectTypeEnum.Conference || 
                     project.ProjectType == (int)ProjectTypeEnum.Journal)
            {
                // For Conference/Journal papers - check if user is an author
                var author = await _context.Authors
                    .FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId && a.UserId == userId);
                    
                if (author == null)
                    throw new ServiceException("You are not an author of this publication");
                
                // Get the user's group membership 
                var userGroupMember = await _context.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.UserId == userId);
                    
                if (userGroupMember == null)
                    throw new ServiceException("You must be a member of a research group to request funds");
                
                // Only allow lecturers to create disbursement requests for publications
                if (!isLecturer)
                    throw new ServiceException("Only lecturers can create fund disbursement requests for publications");
            }
            else
            {
                throw new ServiceException("Unsupported project type for fund disbursement");
            }
            
            // Continue with the rest of the validation...
            // Check active quotas for this project
            var availableQuota = await _context.Quotas
                .Where(q => q.ProjectId == request.ProjectId && q.Status == (int)QuotaStatusEnum.Active)
                .FirstOrDefaultAsync();
                
            if (availableQuota == null)
                throw new ServiceException("No active quota available for this project");
                
            // Calculate already approved/disbursed amount from this quota
            decimal alreadyApprovedAmount = await _context.FundDisbursements
                .Where(fd => fd.QuotaId == availableQuota.QuotaId && 
                           (fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                            fd.Status == (int)FundDisbursementStatusEnum.Disbursed ||
                            fd.Status == (int)FundDisbursementStatusEnum.Pending))
                .SumAsync(fd => fd.FundRequest ?? 0);
                
            // Calculate available budget
            decimal availableBudget = availableQuota.AllocatedBudget.GetValueOrDefault() - alreadyApprovedAmount;
                
            // Check if requested amount exceeds available budget
            if (request.FundRequest > availableBudget)
                throw new ServiceException($"Requested amount exceeds available budget. Available: {availableBudget}, Requested: {request.FundRequest}");
                
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
                    // Check if this completed phase already has an active fund disbursement request
                    // (pending or approved, but not rejected)
                    var existingActiveDisbursement = await _context.FundDisbursements
                        .AnyAsync(f => f.ProjectPhaseId == request.ProjectPhaseId.Value && 
                                  (f.Status == (int)FundDisbursementStatusEnum.Pending || 
                                   f.Status == (int)FundDisbursementStatusEnum.Approved ||
                                   f.Status == (int)FundDisbursementStatusEnum.Disbursed));
                        
                    if (existingActiveDisbursement)
                    {
                        throw new ServiceException("This completed project phase already has an active fund disbursement request. Only one active request is allowed per completed phase. If your previous request was rejected, you can create a new one.");
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
                UserRequest = userId
            };
            
            await _fundDisbursementRepository.AddAsync(fundDisbursement);
            
            // Create corresponding project request
            var projectRequest = new ProjectRequest
            {
                ProjectId = request.ProjectId,
                PhaseId = request.ProjectPhaseId,
                RequestType = ProjectRequestTypeEnum.Fund_Disbursement,
                RequestedById = userId,
                RequestedAt = DateTime.Now,
                ApprovalStatus = ApprovalStatusEnum.Pending,
                FundDisbursementId = fundDisbursement.FundDisbursementId
            };
            
            await _context.ProjectRequests.AddAsync(projectRequest);
            await _context.SaveChangesAsync();
            
            // Commit the transaction
            await transaction.CommitAsync();
            
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
            Console.WriteLine($"Service received {disbursements.Count()} disbursements from repository.");
            return MapToDisbursementResponses(disbursements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in FundDisbursementService.GetAllFundDisbursements: {ex.ToString()}");
            throw new ServiceException($"Error retrieving fund disbursements: {ex.Message}", ex);
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
                
            // Check if user is authorized (the requester)
            if (disbursement.UserRequest != userId)
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
                    Acquired = (bool?)true,
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
    
    public async Task<bool> ApproveFundDisbursement(
        int fundDisbursementId, 
        int approverId,
        IEnumerable<IFormFile> documentFiles)
    {
        try
        {
            // Use a transaction to ensure all database changes succeed or fail together
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Original approval logic (same as before)
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

            // Calculate already approved/disbursed amount from this quota
            decimal alreadyApprovedAmount = await _context.FundDisbursements
                .Where(fd => fd.QuotaId == quota.QuotaId && 
                            fd.FundDisbursementId != fundDisbursementId && 
                            (fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                             fd.Status == (int)FundDisbursementStatusEnum.Disbursed))
                .SumAsync(fd => fd.FundRequest ?? 0);
                
            // Calculate available budget
            decimal availableBudget = quota.AllocatedBudget.GetValueOrDefault() - alreadyApprovedAmount;
                
            // Check if quota has sufficient funds
            if (availableBudget < fundDisbursement.FundRequest)
                throw new ServiceException($"Insufficient quota budget for this disbursement. Available: {availableBudget}, Requested: {fundDisbursement.FundRequest}");
                
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
            
            // Update the fund disbursement
            fundDisbursement.Status = (int)FundDisbursementStatusEnum.Approved;
            fundDisbursement.AppovedBy = groupMember.GroupMemberId;
            fundDisbursement.UpdateAt = DateTime.Now;
            
            // Update the quota's remaining budget
            quota.AllocatedBudget -= fundDisbursement.FundRequest;
            quota.UpdateAt = DateTime.Now;
            
            // If quota is fully used, update its status
            if (quota.AllocatedBudget <= 0)
            {
                quota.Status = (int)QuotaStatusEnum.Used;
            }
            
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
            
            // Save all changes
            await _fundDisbursementRepository.UpdateAsync(fundDisbursement);
            await _context.SaveChangesAsync();
            
            // Upload documents
            var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{fundDisbursement.ProjectId}/disbursements/{fundDisbursementId}/approval");
            int index = 0;
            
            foreach (var file in documentFiles)
            {
                // Create resource
                var projectResource = new ProjectResource
                {
                    ResourceName = file.FileName,
                    ResourceType = 1, // Document
                    ProjectId = fundDisbursement.ProjectId,
                    Acquired = (bool?)true,
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
                    DocumentType = (int)DocumentTypeEnum.DisbursementConfirmation,
                    UploadAt = DateTime.Now,
                    UploadedBy = approverId,
                    ProjectResourceId = projectResource.ProjectResourceId,
                    ProjectId = fundDisbursement.ProjectId
                };
                
                await _context.Documents.AddAsync(document);
                index++;
            }
            
            await _context.SaveChangesAsync();
            
            // Commit the transaction
            await transaction.CommitAsync();
            
            // Create notification for requester
            if (fundDisbursement.UserRequest.HasValue)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = fundDisbursement.UserRequest.Value,
                    Title = "Fund Disbursement Approved",
                    Message = $"Your fund disbursement request of {fundDisbursement.FundRequest:C} has been approved.",
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
    
    public async Task<bool> RejectFundDisbursement(
        int fundDisbursementId, 
        string rejectionReason, 
        int rejectorId,
        IEnumerable<IFormFile> documentFiles)
    {
        try
        {
            // Use a transaction for consistency
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
            fundDisbursement.RejectionReason = rejectionReason; // Save the reason here
            fundDisbursement.UpdateAt = DateTime.Now;
            
            await _fundDisbursementRepository.UpdateAsync(fundDisbursement);
            
            // Upload documents
            var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{fundDisbursement.ProjectId}/disbursements/{fundDisbursementId}/rejection");
            int index = 0;
            
            foreach (var file in documentFiles)
            {
                // Create resource
                var projectResource = new ProjectResource
                {
                    ResourceName = file.FileName,
                    ResourceType = 1, // Document
                    ProjectId = fundDisbursement.ProjectId,
                    Acquired = (bool?)true,
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
                    DocumentType = (int)DocumentTypeEnum.DisbursementConfirmation,
                    UploadAt = DateTime.Now,
                    UploadedBy = rejectorId,
                    ProjectResourceId = projectResource.ProjectResourceId,
                    ProjectId = fundDisbursement.ProjectId
                };
                
                await _context.Documents.AddAsync(document);
                index++;
            }
            
            await _context.SaveChangesAsync();
            
            // Commit the transaction
            await transaction.CommitAsync();
            
            // Create notification for requester
            if (fundDisbursement.UserRequest.HasValue)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = fundDisbursement.UserRequest.Value,
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
                
            // Check if user is authorized
            if (disbursement.UserRequest != userId)
                throw new ServiceException("You are not authorized to upload documents for this disbursement");
                
            // Upload document to S3
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"projects/{disbursement.ProjectId}/disbursements/{fundDisbursementId}");
            
            // Create resource
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document
                ProjectId = disbursement.ProjectId,
                Acquired = (bool?)true,
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
        // Calculate total disbursed amount for the project
        decimal projectDisbursedAmount = 0;
        if (disbursement.Project?.FundDisbursements != null)
        {
            projectDisbursedAmount = disbursement.Project.FundDisbursements
                .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || 
                             fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                .Sum(fd => fd.FundRequest ?? 0);
        }

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
            
            // User information
            RequesterId = disbursement.UserRequest ?? 0,
            RequesterName = disbursement.UserRequestNavigation?.FullName,

            // Map Approver Info (using AppovedBy from entity)
            ApprovedById = disbursement.AppovedByNavigation?.UserId, 
            ApprovedByName = disbursement.AppovedByNavigation?.User?.FullName, 

            // Map Disburser Info
            DisbursedById = disbursement.DisburseBy,
            DisbursedByName = disbursement.DisburseByNavigation?.FullName, 
            
            // Documents
            Documents = disbursement.Documents != null
                ? disbursement.Documents.Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName ?? "Unknown",
                    DocumentUrl = d.DocumentUrl ?? "",
                    DocumentType = d.DocumentType ?? 0,
                    UploadAt = d.UploadAt ?? DateTime.MinValue
                }).ToList()
                : new List<DocumentResponse>(),
            
            // Project additional info
            ProjectType = disbursement.Project?.ProjectType,
            ProjectTypeName = disbursement.Project?.ProjectType.HasValue == true ? 
                Enum.GetName(typeof(ProjectTypeEnum), disbursement.Project.ProjectType.Value) : null,
            ProjectApprovedBudget = disbursement.Project?.ApprovedBudget,
            ProjectSpentBudget = disbursement.Project?.SpentBudget ?? 0,
            ProjectDisbursedAmount = projectDisbursedAmount,
            
            // Project phases
            ProjectPhases = disbursement.Project?.ProjectPhases?.Select(pp => new ProjectPhaseInfo
            {
                ProjectPhaseId = pp.ProjectPhaseId,
                Title = pp.Title,
                StartDate = pp.StartDate,
                EndDate = pp.EndDate,
                Status = pp.Status,
                StatusName = pp.Status.HasValue == true ? 
                    Enum.GetName(typeof(ProjectPhaseStatusEnum), pp.Status.Value) : null,
                SpentBudget = pp.SpentBudget
            }).ToList(),

            // Map the RejectionReason
            RejectionReason = disbursement.RejectionReason
        };
        
        return response;
    }
    
    private IEnumerable<FundDisbursementResponse> MapToDisbursementResponses(IEnumerable<FundDisbursement> disbursements)
    {
        return disbursements.Select(d => MapToDisbursementResponse(d));
    }
}
