using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Implementations;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;
[ApiController]
public class ProjectController : ApiBaseController
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    [HttpGet("project/list-all-project")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllProjects()
    {
        try
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ApiResponse(StatusCodes.Status403Forbidden,"You are not allowed to use this feature"));
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
    [Authorize(Roles = "Admin")]
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
    public async Task<IActionResult> CreateResearchProject([FromForm] CreateProjectRequest request, IFormFile documentFile)
    {
        try
        {
            if (documentFile != null)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Only PDF, DOC, and DOCX files are allowed");
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var projectId = await _projectService.CreateResearchProject(request, documentFile, userId);
            var response = new ApiResponse(StatusCodes.Status200OK, $"Project has been registered. Project ID: {projectId}");
            return Ok(response);
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("project/approval-request")]
    public async Task<IActionResult> SendProjectForApproval([FromBody] ProjectApprovalRequest request)
    {
        try
        {
            var result = await _projectService.SendProjectForApproval(request);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("project/{projectId}/approve")]
    public async Task<IActionResult> ApproveProject(int projectId)
    {
        try
        {
            int currentUserId = int.Parse(User.Identity.Name);
            var result = await _projectService.ApproveProject(projectId, currentUserId);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("project/{projectId}/reject")]
    public async Task<IActionResult> RejectProject(int projectId, [FromBody] string reason)
    {
        try
        {
            int currentUserId = int.Parse(User.Identity.Name);
            var result = await _projectService.RejectProject(projectId, currentUserId, reason);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
