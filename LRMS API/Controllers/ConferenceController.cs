using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;

[ApiController]
public class ConferenceController : ApiBaseController
{
    private readonly IConferenceService _conferenceService;

    public ConferenceController(IConferenceService conferenceService)
    {
        _conferenceService = conferenceService;
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

    [HttpPost("conference/{conferenceId}/upload-document")]
    [Authorize]
    public async Task<IActionResult> UploadConferenceDocuments(int conferenceId, List<IFormFile> documentFiles)
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
            await _conferenceService.AddConferenceDocuments(conferenceId, documentFiles, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
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
} 