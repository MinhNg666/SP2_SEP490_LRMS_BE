using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;

[ApiController]
public class JournalController : ApiBaseController
{
    private readonly IJournalService _journalService;

    public JournalController(IJournalService journalService)
    {
        _journalService = journalService;
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

    [HttpPost("journal/{journalId}/upload-document")]
    [Authorize]
    public async Task<IActionResult> UploadJournalDocument(int journalId, IFormFile documentFile)
    {
        try
        {
            if (documentFile == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "No file uploaded"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Only PDF, DOC, and DOCX files are allowed"));

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.AddJournalDocument(journalId, documentFile, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Document uploaded successfully"));
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
} 