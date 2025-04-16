using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Requests;

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
}
