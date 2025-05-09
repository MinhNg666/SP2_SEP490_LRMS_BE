using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Domain.Constants;
using LRMS_API;
using Microsoft.EntityFrameworkCore;

namespace LRMS_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JournalController : ApiBaseController
{
    private readonly IJournalService _journalService;
    private readonly LRMSDbContext _context;
    private readonly IS3Service _s3Service;
    private readonly IFundDisbursementService _fundDisbursementService;

    public JournalController(
        IJournalService journalService,
        LRMSDbContext context,
        IS3Service s3Service,
        IFundDisbursementService fundDisbursementService)
    {
        _journalService = journalService;
        _context = context;
        _s3Service = s3Service;
        _fundDisbursementService = fundDisbursementService;
    }

    [HttpPost("journal/register")]
    [Authorize]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var journal = await _journalService.CreateJournal(request, userId);
            var response = new ApiResponse(StatusCodes.Status200OK, $"Journal has been registered. Journal ID: {journal.JournalId}", journal);
            return Ok(response);
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpPost("create-journal-from-research/{projectId}")]
    [Authorize]
    public async Task<IActionResult> CreateFromResearch(int projectId, [FromBody] CreateJournalFromProjectRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var journalId = await _journalService.CreateJournalFromResearch(projectId, userId, request);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal created successfully", new { journalId }));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("upload-documents/{journalId}")]
    [Authorize]
    public async Task<IActionResult> UploadJournalDocuments(int journalId, List<IFormFile> documents)
    {
        try
        {
            if (documents == null || !documents.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No documents provided"));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.AddJournalDocuments(journalId, documents, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("approve-journal/{journalId}")]
    [Authorize]
    public async Task<IActionResult> ApproveJournal(int journalId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.ApproveJournal(journalId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Journal đã được phê duyệt thành công" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("reject-journal/{journalId}")]
    [Authorize]
    public async Task<IActionResult> RejectJournal(int journalId, IFormFile documentFile)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.RejectJournal(journalId, secretaryId, documentFile);
            return Ok(new { success = true, message = "Journal đã bị từ chối" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("update-status/{journalId}")]
    [Authorize]
    public async Task<IActionResult> UpdateJournalStatus(int journalId, [FromBody] UpdateJournalStatusRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.UpdateJournalStatus(journalId, request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal status updated successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPut("update-details/{journalId}")]
    [Authorize]
    public async Task<IActionResult> UpdateJournalDetails(int journalId, [FromBody] UpdateJournalDetailsRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.UpdateJournalDetails(journalId, request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal details updated successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("journal/list-all")]
    [Authorize]
    public async Task<IActionResult> GetAllJournals()
    {
        try
        {
            var journals = await _journalService.GetAllJournals();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journals retrieved successfully", journals));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("journal/{journalId}")]
    [Authorize]
    public async Task<IActionResult> GetJournalById(int journalId)
    {
        try
        {
            var journal = await _journalService.GetJournalById(journalId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal retrieved successfully", journal));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("journal/project/{projectId}")]
    [Authorize]
    public async Task<IActionResult> GetJournalsByProjectId(int projectId)
    {
        try
        {
            var journals = await _journalService.GetJournalsByProjectId(projectId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journals retrieved successfully", journals));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("user-journals")]
    [Authorize]
    public async Task<IActionResult> GetUserJournals()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var journals = await _journalService.GetUserJournals(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "User journals retrieved successfully", journals));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("journal-details/{journalId}")]
    [Authorize]
    public async Task<IActionResult> GetJournalDetails(int journalId)
    {
        try
        {
            var journalDetails = await _journalService.GetJournalDetails(journalId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal details retrieved successfully", journalDetails));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("journals/{journalId}/request-funding")]
    [Authorize]
    public async Task<IActionResult> RequestJournalFunding(
        int journalId, 
        [FromBody] RequestJournalFundingRequest request)
    {
        try
        {
            // Verify the journal ID in the route matches the one in the request
            if (journalId != request.JournalId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Journal ID in the route does not match the one in the request"));
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Handle the request without documents first
            var requestId = await _journalService.RequestJournalFundingAsync(userId, request, new List<IFormFile>());
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal funding request created successfully", new { RequestId = requestId }));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("journals/{journalId}/funding-requests/{requestId}/upload-documents")]
    [Authorize]
    public async Task<IActionResult> UploadFundingDocuments(
        int journalId,
        int requestId,
        [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No documents provided"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            // Get the request to ensure it exists and is related to this journal
            var projectRequest = await _context.ProjectRequests
                .Include(r => r.FundDisbursement)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                      r.FundDisbursement != null && 
                                      r.FundDisbursement.JournalId == journalId);
            
            if (projectRequest == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No funding request found for this journal with the given request ID"));
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Upload the documents
            string folderPath = $"projects/{projectRequest.ProjectId}/journals/{journalId}/funding";
            var urls = await _s3Service.UploadFilesAsync(documentFiles, folderPath);
            int index = 0;
            
            foreach (var file in documentFiles)
            {
                // Create resource
                var projectResource = new ProjectResource
                {
                    ResourceName = file.FileName,
                    ResourceType = 1, // Document
                    ProjectId = projectRequest.ProjectId,
                    Acquired = true,
                    Quantity = 1
                };
                
                await _context.ProjectResources.AddAsync(projectResource);
                await _context.SaveChangesAsync();
                
                // Create document
                var document = new Document
                {
                    ProjectId = projectRequest.ProjectId,
                    DocumentUrl = urls[index],
                    FileName = file.FileName,
                    DocumentType = (int)DocumentTypeEnum.JournalFunding,
                    UploadAt = DateTime.UtcNow,
                    UploadedBy = userId,
                    ProjectResourceId = projectResource.ProjectResourceId,
                    FundDisbursementId = projectRequest.FundDisbursementId,
                    RequestId = requestId,
                    JournalId = journalId
                };
                
                await _context.Documents.AddAsync(document);
                index++;
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("funding-requests/{requestId}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveJournalFundingRequest(
        int requestId,
        [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach confirmation documents"));
            
            // Validate file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            // Verify this is a journal funding request
            var projectRequest = await _context.ProjectRequests
                .Include(r => r.FundDisbursement)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                    r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement &&
                                    r.FundDisbursement != null && 
                                    r.FundDisbursement.JournalId.HasValue &&
                                    r.FundDisbursement.FundDisbursementType == (int)FundDisbursementTypeEnum.JournalFunding);
            
            if (projectRequest == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Journal funding request not found"));
            
            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var result = await _journalService.ApproveJournalFundingRequest(requestId, approverId, documentFiles);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal funding request approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("funding-requests/{requestId}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectJournalFundingRequest(
        int requestId,
        [FromForm] string rejectionReason,
        [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach rejection documents"));
            
            // Validate file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            // Verify this is a journal funding request
            var projectRequest = await _context.ProjectRequests
                .Include(r => r.FundDisbursement)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && 
                                    r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement &&
                                    r.FundDisbursement != null && 
                                    r.FundDisbursement.JournalId.HasValue &&
                                    r.FundDisbursement.FundDisbursementType == (int)FundDisbursementTypeEnum.JournalFunding);
            
            if (projectRequest == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Journal funding request not found"));
            
            var rejecterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var result = await _journalService.RejectJournalFundingRequest(requestId, rejecterId, rejectionReason, documentFiles);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal funding request rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("journals/{journalId}/fund-disbursements")]
    [Authorize]
    public async Task<IActionResult> GetJournalFundDisbursements(int journalId)
    {
        try
        {
            var fundDisbursements = await _fundDisbursementService.GetFundDisbursementsByJournalId(journalId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Journal fund disbursements retrieved successfully", fundDisbursements));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
} 