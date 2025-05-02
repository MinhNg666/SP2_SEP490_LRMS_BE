using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;

[Route("api/[controller]")]
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
    [HttpPost("create-journal-from-research/{projectId}")]
    [Authorize]
    public async Task<IActionResult> CreateFromResearch(int projectId, [FromForm] CreateJournalFromProjectRequest request, IFormFile documentFile)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var journalId = await _journalService.CreateJournalFromResearch(projectId, userId, request);
            return Ok(new { success = true, journalId = journalId, message = "Đã tạo Journal thành công" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("add-document/{journalId}")]
    [Authorize]
    public async Task<IActionResult> AddJournalDocument(int journalId, IFormFile documentFile)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _journalService.AddJournalDocument(journalId, userId, documentFile);
            return Ok(new { success = true, message = "Đã thêm tài liệu thành công" });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
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