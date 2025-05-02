using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Implementations;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using LRMS_API;

namespace LRMS_API.Controllers;
[ApiController]
public class ProjectController : ApiBaseController
{
    private readonly IProjectService _projectService;
    private readonly LRMSDbContext _context;
    private readonly IFundDisbursementService _fundDisbursementService;

    public ProjectController(IProjectService projectService, LRMSDbContext context, IFundDisbursementService fundDisbursementService)
    {
        _projectService = projectService;
        _context = context;
        _fundDisbursementService = fundDisbursementService;
    }
    [HttpGet("project/list-all-project")]
    [Authorize(Roles = "Admin,Lecturer,Office")]
    public async Task<IActionResult> GetAllProjects()
    {
        try
        {
            // Get the current user's role
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            
            // Check if the user has one of the allowed roles
            if (!(userRole == SystemRoleEnum.Admin.ToString() || 
                  userRole == SystemRoleEnum.Lecturer.ToString() || 
                  userRole == SystemRoleEnum.Office.ToString()))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ApiResponse(StatusCodes.Status403Forbidden, "You are not allowed to use this feature"));
            }
            
            var projects = await _projectService.GetAllProjects();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Projects retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpGet("project/get-project-by-userId/{userId}")]
    public async Task<IActionResult> GetProjectsByUserId(int userId)
    {
        try
        {
            var projects = await _projectService.GetProjectsByUserId(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, $"Retrieve projects of {userId} successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpGet("project/get-my-projects")]
    [Authorize]
    public async Task<IActionResult> GetMyProjects()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var projects = await _projectService.GetProjectsByUserId(currentUserId);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Retrieve your projects successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpGet("project/get-project-by-projectId/{projectId}")]
    public async Task<IActionResult> GetProjectById(int projectId)
    {
        try
        {
            var project = await _projectService.GetProjectById(projectId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project retrieved successfully", project));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpGet("project/get-project-by-departmentId/{departmentId}")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByDepartment(int departmentId)
    {
        try
        {
            var projects = await _projectService.GetProjectsByDepartmentId(departmentId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    [HttpPost("project/register-research-project")]
    [Authorize]
    public async Task<IActionResult> CreateResearchProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var projectId = await _projectService.CreateResearchProject(request, null, userId);
            var response = new ApiResponse(StatusCodes.Status200OK, $"Project has been registered. Project ID: {projectId}");
            return Ok(response);
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("project/{projectId}/upload-document")]
    public async Task<IActionResult> UploadProjectDocument(int projectId, List<IFormFile> documentFiles)
    {
        try
        {
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No files uploaded"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _projectService.AddProjectDocuments(projectId, documentFiles, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    //[HttpPost("project/approval-request")]
    //public async Task<IActionResult> SendProjectForApproval([FromBody] ProjectApprovalRequest request)
    //{
    //    try
    //    {
    //        var result = await _projectService.SendProjectForApproval(request);
    //        return Ok(new { Success = result });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ex.Message);
    //    }
    //}
    [HttpPost("project/{projectId}/council-approve")]
    [Authorize]
    public async Task<IActionResult> ApproveProjectBySecretary(int projectId, List<IFormFile> documentFiles)
    {
        try
        {
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.ApproveProjectBySecretary(projectId, secretaryId, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project/{projectId}/council-reject")]
    [Authorize]
    public async Task<IActionResult> RejectProjectBySecretary(
        int projectId, 
        [FromForm] string rejectionReason, 
        List<IFormFile> documentFiles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.RejectProjectBySecretary(projectId, secretaryId, rejectionReason, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("project/details/{projectId}")]
    public async Task<IActionResult> GetProjectDetails(int projectId)
    {
        try
        {
            var projectDetails = await _projectService.GetProjectDetails(projectId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project details retrieved successfully", projectDetails));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/{userId}/projects/pending")]
    public async Task<IActionResult> GetUserPendingProjects(int userId)
    {
        try
        {
            var projects = await _projectService.GetUserPendingProjectsList(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Pending projects retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/{userId}/projects/approved")]
    public async Task<IActionResult> GetUserApprovedProjects(int userId)
    {
        try
        {
            var projects = await _projectService.GetUserApprovedProjectsList(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Approved projects retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("projects/me/pending")]
    [Authorize]
    public async Task<IActionResult> GetMyPendingProjects()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var projects = await _projectService.GetUserPendingProjectsList(currentUserId);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Your pending projects retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("projects/me/approved")]
    [Authorize]
    public async Task<IActionResult> GetMyApprovedProjects()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var projects = await _projectService.GetUserApprovedProjectsList(currentUserId);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Your approved projects retrieved successfully", projects));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPut("project-phases/{projectPhaseId}")]
    [Authorize]
    public async Task<IActionResult> UpdateProjectPhase(int projectPhaseId, [FromBody] UpdateProjectPhaseRequest request)
    {
        try
        {
            if (projectPhaseId != request.ProjectPhaseId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Project phase IDs do not match"));
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.UpdateProjectPhase(
                projectPhaseId, 
                request.Status, 
                request.SpentBudget,
                request.StartDate,
                request.EndDate,
                request.Title,
                userId);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project phase updated successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project-phases/update-statuses")]
    [Authorize]
    public async Task<IActionResult> UpdateProjectPhaseStatuses()
    {
        try
        {
            await _projectService.UpdateProjectPhaseStatusesBasedOnDates();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project phase statuses updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project/{projectId}/mark-completed")]
    [Authorize]
    public async Task<IActionResult> MarkProjectAsCompleted(int projectId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.MarkProjectAsCompleted(projectId, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project marked as completed successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project/{projectId}/request-completion")]
    [Authorize]
    public async Task<IActionResult> RequestProjectCompletion(int projectId, [FromBody] RequestProjectCompletionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Validation failed", errors));
        }

        if (!request.BudgetReconciled)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Budget reconciliation confirmation is required."));
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not found or not authenticated."));
            }

            // Pass null for documents since we'll handle them separately
            await _projectService.RequestProjectCompletionAsync(projectId, userId, request, null);

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project completion request submitted successfully."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in RequestProjectCompletion: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred."));
        }
    }

    [HttpPost("project/{projectId}/upload-completion-documents")]
    [Authorize]
    public async Task<IActionResult> UploadCompletionDocuments(int projectId, [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No files uploaded"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _projectService.AddCompletionDocumentsAsync(projectId, documentFiles, userId);
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Completion documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in UploadCompletionDocuments: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred."));
        }
    }

    [HttpGet("completion-requests")]
    [Authorize]
    public async Task<IActionResult> GetCompletionRequests()
    {
        try
        {
            var requests = await _projectService.GetCompletionRequestsAsync();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Completion requests retrieved successfully", requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("completion-requests/{requestId}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveCompletionRequest(int requestId, [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.ApproveCompletionRequestAsync(requestId, approverId, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Completion request approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("completion-requests/{requestId}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectCompletionRequest(int requestId, [FromForm] string rejectionReason, [FromForm] List<IFormFile> documentFiles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
            var rejecterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.RejectCompletionRequestAsync(requestId, rejecterId, rejectionReason, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Completion request rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("completion-requests/{requestId}/details")]
    [Authorize]
    public async Task<IActionResult> GetCompletionRequestDetails(int requestId)
    {
        try
        {
            var requestDetails = await _projectService.GetCompletionRequestByIdAsync(requestId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Completion request details retrieved successfully", requestDetails));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("completion-requests/user")]
    [Authorize]
    public async Task<IActionResult> GetUserCompletionRequests()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var requests = await _projectService.GetUserCompletionRequestsAsync(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "User completion requests retrieved successfully", requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project-requests/{requestId}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveProjectRequest(int requestId, List<IFormFile> documentFiles)
    {
        try
        {
            // Validate documents are provided
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "At least one document file is required"));
            
            // Validate file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.ApproveProjectRequestAsync(requestId, secretaryId, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project request approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project-requests/{requestId}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectProjectRequest(int requestId, [FromForm] string rejectionReason, List<IFormFile> documentFiles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
            // Validate documents are provided
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "At least one document file is required"));
            
            // Validate file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            var secretaryId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _projectService.RejectProjectRequestAsync(requestId, secretaryId, rejectionReason, documentFiles);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project request rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("project-requests")]
    [Authorize]
    public async Task<IActionResult> GetAllProjectRequests()
    {
        try
        {
            var requests = await _projectService.GetAllProjectRequestsAsync();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project requests retrieved successfully", requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("departments/{departmentId}/project-requests")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentProjectRequests(int departmentId)
    {
        try
        {
            var requests = await _projectService.GetDepartmentProjectRequestsAsync(departmentId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Department project requests retrieved successfully", requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("project-requests/{requestId}/assign-timeline/{timelineId}")]
    [Authorize]
    public async Task<IActionResult> AssignTimelineToRequest(int requestId, int timelineId)
    {
    try {
        var result = await _projectService.AssignTimelineToRequestAsync(requestId, timelineId);
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Timeline assigned to request successfully"));
    }
    catch (ServiceException ex) {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("departments/{departmentId}/assign-timeline/{timelineId}")]
    [Authorize]
    public async Task<IActionResult> AssignTimelineToDepartmentRequests(
        int departmentId, 
        int timelineId, 
        [FromQuery] ProjectRequestTypeEnum? requestType = null)
    {
        try {
            var count = await _projectService.AssignTimelineToDepartmentRequestsAsync(departmentId, timelineId, requestType);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Timeline assigned to {count} project requests in department successfully"));
        }
        catch (ServiceException ex) {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("fund-disbursement-requests")]
    [Authorize]
    public async Task<IActionResult> GetFundDisbursementRequests()
    {
        try
        {
            var requests = await _projectService.GetAllProjectRequestsAsync();
            var fundRequests = requests.Where(r => r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement);
            
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {fundRequests.Count()} fund disbursement requests", 
                fundRequests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("project-requests/{requestId}/details")]
    [Authorize]
    public async Task<IActionResult> GetProjectRequestDetails(int requestId)
    {
        try
        {
            var requestDetails = await _projectService.GetProjectRequestDetailsAsync(requestId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project request details retrieved successfully", requestDetails));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("departments/{departmentId}/project-requests/pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingDepartmentRequests(int departmentId)
    {
        try
        {
            var requests = await _projectService.GetPendingDepartmentRequestsAsync(departmentId);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {requests.Count()} pending project requests for department", 
                requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/me/project-requests")]
    [Authorize]
    public async Task<IActionResult> GetMyProjectRequests()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var requests = await _projectService.GetUserProjectRequestsAsync(userId);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {requests.Count()} project requests", 
                requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/me/project-requests/pending")]
    [Authorize]
    public async Task<IActionResult> GetMyPendingProjectRequests()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var requests = await _projectService.GetUserPendingProjectRequestsAsync(userId);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {requests.Count()} pending project requests", 
                requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/{userId}/project-requests")]
    [Authorize]
    public async Task<IActionResult> GetUserProjectRequests(int userId)
    {
        try
        {
            var requests = await _projectService.GetUserProjectRequestsAsync(userId);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {requests.Count()} project requests for user", 
                requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("users/{userId}/project-requests/pending")]
    [Authorize]
    public async Task<IActionResult> GetUserPendingProjectRequests(int userId)
    {
        try
        {
            var requests = await _projectService.GetUserPendingProjectRequestsAsync(userId);
            return Ok(new ApiResponse(
                StatusCodes.Status200OK, 
                $"Found {requests.Count()} pending project requests for user", 
                requests));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpGet("projects/{projectId}/completion-summary")]
    [Authorize]
    public async Task<IActionResult> GetProjectCompletionSummary(int projectId)
    {
        try
        {
            var summary = await _projectService.GetProjectCompletionSummaryAsync(projectId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Project completion summary retrieved successfully", summary));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("fund-requests/{requestId}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveFundDisbursementByRequestId(
        int requestId, 
        List<IFormFile> documentFiles)
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
            
            // Find the associated fund disbursement ID
            var projectRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement);
            
            if (projectRequest == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Project request not found or is not a fund disbursement request"));
            
            if (!projectRequest.FundDisbursementId.HasValue)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No fund disbursement associated with this request"));
            
            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
            var result = await _fundDisbursementService.ApproveFundDisbursement(
                projectRequest.FundDisbursementId.Value,
                approverId,
                documentFiles);
            
            // Also update the project request status
            projectRequest.ApprovalStatus = ApprovalStatusEnum.Approved;
            projectRequest.ApprovedById = approverId;
            projectRequest.ApprovedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [HttpPost("fund-requests/{requestId}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectFundDisbursementByRequestId(
        int requestId, 
        [FromForm] string rejectionReason,
        List<IFormFile> documentFiles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Rejection reason is required"));
            
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach rejection documents"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
            
            // Find the associated fund disbursement ID
            var projectRequest = await _context.ProjectRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.RequestType == ProjectRequestTypeEnum.Fund_Disbursement);
            
            if (projectRequest == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Project request not found or is not a fund disbursement request"));
            
            if (!projectRequest.FundDisbursementId.HasValue)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No fund disbursement associated with this request"));
            
            var rejectorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
            var result = await _fundDisbursementService.RejectFundDisbursement(
                projectRequest.FundDisbursementId.Value,
                rejectionReason,
                rejectorId,
                documentFiles);
            
            // Also update the project request status
            projectRequest.ApprovalStatus = ApprovalStatusEnum.Rejected;
            projectRequest.ApprovedById = rejectorId;
            projectRequest.ApprovedAt = DateTime.Now;
            projectRequest.RejectionReason = rejectionReason;
            await _context.SaveChangesAsync();
            
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
