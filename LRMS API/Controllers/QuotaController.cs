using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LRMS_API.Controllers
{
    [ApiController]
    public class QuotaController : ApiBaseController
    {
        private readonly IQuotaService _quotaService;

        public QuotaController(IQuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        [HttpGet("quotas")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> GetAllQuotas()
        {
            try
            {
                var quotas = await _quotaService.GetAllQuotas();
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Quotas retrieved successfully", quotas));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet("my-quotas")]
        [Authorize]
        public async Task<IActionResult> GetMyQuotas()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var quotas = await _quotaService.GetQuotasByUserId(userId);
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Your quotas retrieved successfully", quotas));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
