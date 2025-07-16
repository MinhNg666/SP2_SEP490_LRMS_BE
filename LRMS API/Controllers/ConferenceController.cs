using Domain.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

namespace LRMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferenceController : ApiBaseController
    {
        private readonly IConferenceService _conferenceService;

        public ConferenceController(IConferenceService conferenceService)
        {
            _conferenceService = conferenceService;
        }

        [HttpPost("create-conference-from-research/{projectId}")]
        [Authorize]
        public async Task<IActionResult> CreateFromResearch(int projectId, [FromForm] CreateConferenceFromProjectRequest request, IFormFile documentFile)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var conferenceId = await _conferenceService.CreateConferenceFromResearch(projectId, userId, request, documentFile);
                return Ok(new { success = true, conferenceId = conferenceId, message = "Đã tạo Conference thành công" });
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
    }
}
