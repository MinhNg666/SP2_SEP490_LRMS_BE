﻿using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Interfaces;

namespace LRMS_API.Controllers;
[ApiController]
public class ProjectController : ApiBaseController
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost("research")]
    public async Task<IActionResult> CreateResearchProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            int currentUserId = int.Parse(User.Identity.Name); // Lấy từ token
            var projectId = await _projectService.CreateResearchProject(request, currentUserId);
            return Ok(new { ProjectId = projectId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("approval-request")]
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

    [HttpPost("{projectId}/approve")]
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

    [HttpPost("{projectId}/reject")]
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
