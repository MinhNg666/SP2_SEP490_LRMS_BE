using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Domain.Constants;
using LRMS_API;
using Microsoft.EntityFrameworkCore;

namespace LRMS_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConferenceController : ApiBaseController
{
private readonly IConferenceService _conferenceService;
private readonly LRMSDbContext _context;
private readonly IS3Service _s3Service;
private readonly IFundDisbursementService _fundDisbursementService;

public ConferenceController(
    IConferenceService conferenceService, 
    LRMSDbContext context, 
    IS3Service s3Service,
    IFundDisbursementService fundDisbursementService)
{
    _conferenceService = conferenceService;
    _context = context;
    _s3Service = s3Service;
    _fundDisbursementService = fundDisbursementService;
}

[HttpPost("conference/register")]
[Authorize]
public async Task<IActionResult> CreateConference([FromBody] CreateConferenceRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conference = await _conferenceService.CreateConference(request, userId);
        var response = new ApiResponse(StatusCodes.Status200OK, $"Conference has been registered. Conference ID: {conference.ConferenceId}", conference);
        return Ok(response);
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}
[HttpPost("create-conference-from-research/{projectId}")]
[Authorize]
public async Task<IActionResult> CreateFromResearch(int projectId, [FromBody] CreateConferenceFromProjectRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conferenceId = await _conferenceService.CreateConferenceFromResearch(projectId, userId, request);
        return Ok(new { success = true, conferenceId = conferenceId, message = "Conference registration submitted successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("conferences/{conferenceId}/upload-documents")]
[Authorize]
public async Task<IActionResult> UploadConferenceDocuments(int conferenceId, [FromForm] List<IFormFile> documentFiles)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.AddConferenceDocuments(conferenceId, documentFiles, userId);
        return Ok(new { success = true, message = "Documents uploaded successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

    [HttpPost("approve-conference/{conferenceId}")]
    [Authorize]
    public async Task<IActionResult> ApproveConference(int conferenceId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _conferenceService.ApproveConference(conferenceId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Conference đã được phê duyệt thành công" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("reject-conference/{conferenceId}")]
    [Authorize]
    public async Task<IActionResult> RejectConference(int conferenceId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _conferenceService.RejectConference(conferenceId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Conference đã bị từ chối" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }





[HttpGet("conference/list-all")]
[Authorize]
public async Task<IActionResult> GetAllConferences()
{
    try
    {
        var conferences = await _conferenceService.GetAllConferences();
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conference/{conferenceId}")]
[Authorize]
public async Task<IActionResult> GetConferenceById(int conferenceId)
{
    try
    {
        var conference = await _conferenceService.GetConferenceById(conferenceId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference retrieved successfully", conference));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conference/project/{projectId}")]
[Authorize]
public async Task<IActionResult> GetConferencesByProjectId(int projectId)
{
    try
    {
        var conferences = await _conferenceService.GetConferencesByProjectId(projectId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPut("conferences/{conferenceId}/update-submission")]
[Authorize]
public async Task<IActionResult> UpdateConferenceSubmission(int conferenceId, [FromBody] UpdateConferenceSubmissionRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateConferenceSubmission(conferenceId, userId, request);
        return Ok(new { success = true, message = "Conference submission updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPut("conferences/{conferenceId}/update-status")]
[Authorize]
public async Task<IActionResult> UpdateConferenceStatus(int conferenceId, [FromBody] int status)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateConferenceStatus(conferenceId, userId, status);
        return Ok(new { success = true, message = "Conference status updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}
[HttpGet("my-conferences")]
[Authorize]
public async Task<IActionResult> GetMyConferences()
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var conferences = await _conferenceService.GetUserConferences(userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "User conferences retrieved successfully", conferences));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conferences/{conferenceId}/details")]
[Authorize]
public async Task<IActionResult> GetConferenceDetails(int conferenceId)
{
    try
    {
        var details = await _conferenceService.GetConferenceDetails(conferenceId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference details retrieved successfully", details));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPut("conferences/{conferenceId}/submission-status")]
[Authorize]
public async Task<IActionResult> UpdateSubmissionStatus(int conferenceId, [FromBody] UpdateSubmissionStatusRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateSubmissionStatus(conferenceId, userId, request.SubmissionStatus, request.ReviewerComment);
        return Ok(new { success = true, message = "Conference submission status updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPut("conferences/{conferenceId}/approved-details")]
[Authorize]
public async Task<IActionResult> UpdateApprovedConferenceDetails(int conferenceId, [FromBody] UpdateApprovedConferenceRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _conferenceService.UpdateApprovedConferenceDetails(conferenceId, userId, request);
        return Ok(new { success = true, message = "Approved conference details updated successfully" });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("projects/{projectId}/conferences/{rejectedConferenceId}/new-submission")]
[Authorize]
public async Task<IActionResult> CreateNewSubmissionAfterRejection(
    int projectId, 
    int rejectedConferenceId, 
    [FromBody] CreateNewSubmissionRequest request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var newConferenceId = await _conferenceService.CreateNewSubmissionAfterRejection(
            projectId, userId, rejectedConferenceId, request);
        return Ok(new { 
            success = true, 
            message = "New conference submission created successfully", 
            conferenceId = newConferenceId 
        });
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
}

[HttpPost("conferences/{conferenceId}/request-expense")]
[Authorize]
public async Task<IActionResult> RequestConferenceExpense(
    int conferenceId,
    [FromBody] RequestConferenceExpenseRequest request)
{
    try
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Accommodation))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Accommodation details are required"));
            
        if (string.IsNullOrWhiteSpace(request.Travel))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Travel details are required"));
            
        if (request.AccommodationExpense < 0)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Accommodation expense cannot be negative"));
            
        if (request.TravelExpense < 0)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Travel expense cannot be negative"));
        
        // Set the conference ID from the route parameter
        request.ConferenceId = conferenceId;
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var requestId = await _conferenceService.RequestConferenceExpenseAsync(userId, request, null);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference expense request submitted successfully", new { RequestId = requestId }));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("expense-requests/pending")]
[Authorize]
public async Task<IActionResult> GetPendingConferenceExpenseRequests()
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users.FindAsync(userId);
        
        // Office role is 4, Accounting_Department role is 3
        if (user == null || (user.Role != 4 && user.Role != 3))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You are not authorized to view expense requests"));
        
        var requests = await _conferenceService.GetPendingConferenceExpenseRequestsAsync();
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Pending conference expense requests retrieved successfully", requests));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPost("expense-requests/{requestId}/approve")]
[Authorize]
public async Task<IActionResult> ApproveConferenceExpense(int requestId, List<IFormFile> documentFiles)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users.FindAsync(userId);
        
        // Office role is 4, Accounting_Department role is 3
        if (user == null || (user.Role != 4 && user.Role != 3))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You are not authorized to approve expense requests"));
        
        // Validate document files
        if (documentFiles == null || !documentFiles.Any())
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach approval documents"));
        
        // Validate file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var file in documentFiles)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
        }
        
        // Approved status is 1
        await _conferenceService.UpdateConferenceExpenseStatus(requestId, 1, null, documentFiles, userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference expense request approved successfully"));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPost("expense-requests/{requestId}/reject")]
[Authorize]
public async Task<IActionResult> RejectConferenceExpense(int requestId, [FromForm] string rejectionReason, [FromForm] List<IFormFile> documentFiles)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users.FindAsync(userId);
        
        // Office role is 4, Accounting_Department role is 3
        if (user == null || (user.Role != 4 && user.Role != 3))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You are not authorized to reject expense requests"));
            
        if (string.IsNullOrWhiteSpace(rejectionReason))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
        // Validate document files
        if (documentFiles == null || !documentFiles.Any())
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach rejection documents"));
        
        // Validate file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var file in documentFiles)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
        }
        
        // First update the project request
        var request = await _context.ProjectRequests.FindAsync(requestId);
        if (request != null)
        {
            request.RejectionReason = rejectionReason;
            _context.ProjectRequests.Update(request);
            await _context.SaveChangesAsync();
        }
        
        // Then also update the fund disbursement
        if (request?.FundDisbursementId.HasValue == true)
        {
            var fundDisbursement = await _context.FundDisbursements.FindAsync(request.FundDisbursementId.Value);
            if (fundDisbursement != null)
            {
                fundDisbursement.RejectionReason = rejectionReason;
                _context.FundDisbursements.Update(fundDisbursement);
                await _context.SaveChangesAsync();
            }
        }
        
        // Rejected status is 2
        await _conferenceService.UpdateConferenceExpenseStatus(requestId, 2, rejectionReason, documentFiles, userId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference expense request rejected successfully"));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conferences/{conferenceId}/expenses")]
[Authorize]
public async Task<IActionResult> GetConferenceExpenses(int conferenceId)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // Check if the conference exists
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
        if (conference == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, $"Conference with ID {conferenceId} not found"));
            
        // Get the expenses with their rejection reasons
        var expenses = await _conferenceService.GetConferenceExpensesAsync(conferenceId);
        
        // Verify each expense has proper rejection reason if it's rejected
        foreach (var expense in expenses)
        {
            if (expense.ExpenseStatus == (int)ConferenceExpenseStatusEnum.Rejected && 
                string.IsNullOrWhiteSpace(expense.RejectionReason))
            {
                // Log this inconsistency but don't fail the request
                Console.WriteLine($"Warning: Expense ID {expense.ExpenseId} is rejected but has no rejection reason");
            }
        }
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference expenses retrieved successfully", expenses));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
    catch (Exception ex)
    {
        // Log the unexpected error
        Console.WriteLine($"Unexpected error in GetConferenceExpenses: {ex.Message}");
        return StatusCode(StatusCodes.Status500InternalServerError, 
            new ApiResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred"));
    }
}

[HttpPost("conferences/{conferenceId}/request-funding")]
[Authorize]
public async Task<IActionResult> RequestConferenceFunding(
    int conferenceId, 
    [FromBody] RequestConferenceFundingRequest request)
{
    try
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Location))
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Location is required"));
            
        if (request.ConferenceFunding <= 0)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Conference funding amount must be greater than zero"));
        
        // Set the conference ID from the route parameter
        request.ConferenceId = conferenceId;
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var requestId = await _conferenceService.RequestConferenceFundingAsync(userId, request, null);
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference funding request submitted successfully", new { RequestId = requestId }));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPost("conferences/{conferenceId}/expense-requests/{requestId}/upload-documents")]
[Authorize]
public async Task<IActionResult> UploadExpenseDocuments(
    int conferenceId,
    int requestId,
    [FromForm] List<IFormFile> documentFiles)
{
    try
    {
        // Validate document files
        if (documentFiles == null || !documentFiles.Any())
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach supporting documents"));
        
        // Validate file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var file in documentFiles)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // Get the project ID from the request
        var projectRequest = await _context.ProjectRequests.FindAsync(requestId);
        if (projectRequest == null)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request not found"));
        
        int projectId = projectRequest.ProjectId;
        
        // Verify the conference exists and belongs to the project
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId && c.ProjectId == projectId);
            
        if (conference == null)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Conference not found or does not belong to the project"));
        
        // Upload documents to S3
        string folderPath = $"projects/{projectId}/conferences/{conferenceId}/expense-requests";
        var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
        
        // Get fund disbursement ID from the request
        var fundDisbursementId = projectRequest.FundDisbursementId;
        if (!fundDisbursementId.HasValue)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Fund disbursement not found for this request"));
            
        // Save documents in database
        for (int i = 0; i < documentFiles.Count; i++)
        {
            var file = documentFiles[i];
            
            // Create resource
            var projectResource = new ProjectResource
            {
                ResourceName = file.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();
            
            // Create document
            var document = new Document
            {
                ProjectId = projectId,
                DocumentUrl = urls[i],
                FileName = file.FileName,
                DocumentType = (int)DocumentTypeEnum.ConferenceExpense, // Conference Expense document type
                UploadAt = DateTime.UtcNow,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId,
                FundDisbursementId = fundDisbursementId,
                RequestId = requestId,
                ConferenceId = conferenceId
            };
            
            await _context.Documents.AddAsync(document);
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference expense documents uploaded successfully"));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpPost("conferences/{conferenceId}/funding-requests/{requestId}/upload-documents")]
[Authorize]
public async Task<IActionResult> UploadFundingDocuments(
    int conferenceId,
    int requestId,
    [FromForm] List<IFormFile> documentFiles)
{
    try
    {
        // Validate document files
        if (documentFiles == null || !documentFiles.Any())
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach supporting documents"));
        
        // Validate file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        foreach (var file in documentFiles)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // Get the project ID from the request
        var projectRequest = await _context.ProjectRequests.FindAsync(requestId);
        if (projectRequest == null)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Request not found"));
        
        int projectId = projectRequest.ProjectId;
        
        // Verify the conference exists and belongs to the project
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId && c.ProjectId == projectId);
            
        if (conference == null)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Conference not found or does not belong to the project"));
        
        // Upload documents to S3
        string folderPath = $"projects/{projectId}/conferences/{conferenceId}/funding";
        var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
        
        // Get fund disbursement ID from the request
        var fundDisbursementId = projectRequest.FundDisbursementId;
        if (!fundDisbursementId.HasValue)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Fund disbursement not found for this request"));
            
        // Save documents in database
        for (int i = 0; i < documentFiles.Count; i++)
        {
            var file = documentFiles[i];
            
            // Create resource
            var projectResource = new ProjectResource
            {
                ResourceName = file.FileName,
                ResourceType = 1, // Document
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };
            
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();
            
            // Create document
            var document = new Document
            {
                ProjectId = projectId,
                DocumentUrl = urls[i],
                FileName = file.FileName,
                DocumentType = (int)DocumentTypeEnum.ConferenceFunding, // Conference Funding document type
                UploadAt = DateTime.UtcNow,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId,
                FundDisbursementId = fundDisbursementId,
                RequestId = requestId,
                ConferenceId = conferenceId
            };
            
            await _context.Documents.AddAsync(document);
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Conference funding documents uploaded successfully"));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conferences/{conferenceId}/expense-debug/{expenseId}")]
[Authorize]
public async Task<IActionResult> GetExpenseDebugInfo(int conferenceId, int expenseId)
{
    try
    {
        // Get the expense
        var expense = await _context.ConferenceExpenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.ConferenceId == conferenceId);
            
        if (expense == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, $"Expense with ID {expenseId} not found for conference {conferenceId}"));
            
        // Get all fund disbursements related to this conference
        var fundDisbursements = await _context.FundDisbursements
            .Where(fd => fd.ConferenceId == conferenceId && 
                  fd.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceExpense)
            .OrderByDescending(fd => fd.CreatedAt)
            .ToListAsync();
            
        // Get all project requests related to these fund disbursements
        var fundDisbursementIds = fundDisbursements.Select(fd => fd.FundDisbursementId).ToList();
        var projectRequests = await _context.ProjectRequests
            .Where(pr => pr.FundDisbursementId.HasValue && 
                  fundDisbursementIds.Contains(pr.FundDisbursementId.Value))
            .ToListAsync();
            
        // Get all documents related to this expense
        var documents = await _context.Documents
            .Where(d => (d.ConferenceExpenseId == expenseId || d.ConferenceId == conferenceId) &&
                  (d.DocumentType == (int)DocumentTypeEnum.ConferenceExpense ||
                   d.DocumentType == (int)DocumentTypeEnum.ConferenceExpenseDecision))
            .ToListAsync();
            
        // Build a diagnostic object
        var diagnosticInfo = new
        {
            Expense = expense,
            RejectionInfo = new
            {
                FundDisbursements = fundDisbursements.Select(fd => new 
                {
                    fd.FundDisbursementId,
                    fd.Status,
                    fd.RejectionReason,
                    fd.CreatedAt,
                    ProjectRequests = projectRequests
                        .Where(pr => pr.FundDisbursementId == fd.FundDisbursementId)
                        .Select(pr => new
                        {
                            pr.RequestId,
                            pr.ApprovalStatus,
                            pr.RejectionReason,
                            pr.RequestedAt
                        })
                }),
                Documents = documents.Select(d => new
                {
                    d.DocumentId,
                    d.DocumentType,
                    d.FileName,
                    d.ConferenceExpenseId,
                    d.FundDisbursementId,
                    d.RequestId
                })
            }
        };
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Expense debug info retrieved successfully", diagnosticInfo));
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, 
            new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"));
    }
}

[HttpPost("conferences/fix-expense-rejection/{expenseId}")]
[Authorize]
public async Task<IActionResult> FixExpenseRejectionReason(int expenseId, [FromBody] string rejectionReason)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users.FindAsync(userId);
        
        // Only authorized users can fix rejection reasons
        if (user == null || (user.Role != 4 && user.Role != 3)) // Office or Accounting roles
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You are not authorized to modify rejection reasons"));
        
        // Get the expense
        var expense = await _context.ConferenceExpenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
            
        if (expense == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, $"Expense with ID {expenseId} not found"));
            
        if (expense.ExpenseStatus != (int)ConferenceExpenseStatusEnum.Rejected)
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "This expense is not rejected, cannot set rejection reason"));
            
        // Get the conference
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == expense.ConferenceId);
            
        if (conference == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Conference not found for this expense"));
            
        // Get fund disbursements related to this conference
        var fundDisbursements = await _context.FundDisbursements
            .Where(fd => fd.ConferenceId == expense.ConferenceId && 
                  fd.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceExpense)
            .OrderByDescending(fd => fd.CreatedAt) // Get most recent first
            .ToListAsync();
            
        // Get project requests related to these fund disbursements
        var fundDisbursementIds = fundDisbursements.Select(fd => fd.FundDisbursementId).ToList();
        var projectRequests = await _context.ProjectRequests
            .Where(pr => pr.FundDisbursementId.HasValue && 
                  fundDisbursementIds.Contains(pr.FundDisbursementId.Value))
            .ToListAsync();
            
        // Get the fund disbursement index that would correspond to this expense index
        var sortedExpenses = await _context.ConferenceExpenses
            .Where(e => e.ConferenceId == expense.ConferenceId)
            .OrderBy(e => e.ExpenseId)
            .ToListAsync();
            
        var expenseIndex = sortedExpenses.FindIndex(e => e.ExpenseId == expenseId);
        
        // If we have a corresponding fund disbursement, update its rejection reason
        if (expenseIndex >= 0 && expenseIndex < fundDisbursements.Count)
        {
            var fundDisbursement = fundDisbursements[expenseIndex];
            fundDisbursement.RejectionReason = rejectionReason;
            _context.FundDisbursements.Update(fundDisbursement);
            
            // Also update the project request if it exists
            var projectRequest = projectRequests
                .FirstOrDefault(pr => pr.FundDisbursementId == fundDisbursement.FundDisbursementId);
                
            if (projectRequest != null)
            {
                projectRequest.RejectionReason = rejectionReason;
                _context.ProjectRequests.Update(projectRequest);
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Rejection reason updated successfully", new 
            { 
                ExpenseId = expenseId, 
                UpdatedRejectionReason = rejectionReason 
            }));
        }
        else
        {
            // Direct fix - create a new fund disbursement entry specifically for this expense
            var newFundDisbursement = new FundDisbursement
            {
                ConferenceId = expense.ConferenceId,
                ProjectId = conference.ProjectId,
                Status = (int)FundDisbursementStatusEnum.Rejected,
                RejectionReason = rejectionReason,
                CreatedAt = DateTime.UtcNow,
                UserRequest = userId,
                FundDisbursementType = (int)FundDisbursementTypeEnum.ConferenceExpense,
                Description = $"Manual fix for expense ID {expenseId}"
            };
            
            await _context.FundDisbursements.AddAsync(newFundDisbursement);
            await _context.SaveChangesAsync();
            
            // Create a new project request linked to this fund disbursement
            var newProjectRequest = new ProjectRequest
            {
                ProjectId = conference.ProjectId.Value,
                RequestType = ProjectRequestTypeEnum.Fund_Disbursement,
                RequestedById = userId,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatusEnum.Rejected,
                RejectionReason = rejectionReason,
                FundDisbursementId = newFundDisbursement.FundDisbursementId
            };
            
            await _context.ProjectRequests.AddAsync(newProjectRequest);
            await _context.SaveChangesAsync();
            
            // Create a document link between the expense and the fund disbursement
            var document = new Document
            {
                ProjectId = conference.ProjectId,
                DocumentType = (int)DocumentTypeEnum.ConferenceExpenseDecision,
                FileName = $"Expense_{expenseId}_Rejection.txt",
                DocumentUrl = "manual-fix",
                UploadAt = DateTime.UtcNow,
                UploadedBy = userId,
                ConferenceId = expense.ConferenceId,
                ConferenceExpenseId = expenseId,
                FundDisbursementId = newFundDisbursement.FundDisbursementId,
                RequestId = newProjectRequest.RequestId
            };
            
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Created new rejection record successfully", new 
            { 
                ExpenseId = expenseId, 
                FundDisbursementId = newFundDisbursement.FundDisbursementId,
                ProjectRequestId = newProjectRequest.RequestId,
                RejectionReason = rejectionReason 
            }));
        }
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, 
            new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"));
    }
}

[HttpGet("conferences/{conferenceId}/fund-disbursements")]
[Authorize]
public async Task<IActionResult> GetFundDisbursements(int conferenceId)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // Check if the conference exists
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
        if (conference == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, $"Conference with ID {conferenceId} not found"));
            
        // Get the fund disbursements for this conference
        var fundDisbursements = await _fundDisbursementService.GetFundDisbursementsByConferenceId(conferenceId);
        
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursements retrieved successfully", fundDisbursements));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

[HttpGet("conferences/{conferenceId}/funding")]
[Authorize]
public async Task<IActionResult> GetConferenceFunding(int conferenceId)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // Check if the conference exists
        var conference = await _context.Conferences
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
        if (conference == null)
            return NotFound(new ApiResponse(StatusCodes.Status404NotFound, $"Conference with ID {conferenceId} not found"));
            
        // Get all fund disbursements for this conference
        var allDisbursements = await _fundDisbursementService.GetFundDisbursementsByConferenceId(conferenceId);
        
        // Filter to only get conference funding (type 2)
        var fundingDisbursements = allDisbursements
            .Where(d => d.FundDisbursementType == (int)FundDisbursementTypeEnum.ConferenceFunding)
            .ToList();
        
        return Ok(new ApiResponse(
            StatusCodes.Status200OK, 
            $"Found {fundingDisbursements.Count} conference funding disbursements", 
            fundingDisbursements));
    }
    catch (ServiceException ex)
    {
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
    }
}

} 