using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;
using Domain.DTO.Requests;

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

        [HttpGet("quotas/{quotaId}")]
        [Authorize]
        public async Task<IActionResult> GetQuotaById(int quotaId)
        {
            try
            {
                var quotaDetail = await _quotaService.GetQuotaDetailById(quotaId);
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Quota details retrieved successfully", quotaDetail));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("department-quotas")]
        [Authorize]
        public async Task<IActionResult> AllocateQuotaToDepartment([FromBody] AllocateDepartmentQuotaRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var quotaId = await _quotaService.AllocateQuotaToDepartment(request, userId);
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Quota allocated to department successfully", quotaId));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet("department-quotas")]
        [Authorize(Roles = "Admin,Office,Lecturer")]
        public async Task<IActionResult> GetDepartmentQuotas()
        {
            try
            {
                var quotas = await _quotaService.GetDepartmentQuotas();
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Department quotas retrieved successfully", quotas));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet("department-quotas/{departmentId}")]
        [Authorize(Roles = "Admin,Office,Lecturer")]
        public async Task<IActionResult> GetQuotasByDepartment(int departmentId)
        {
            try
            {
                var quotas = await _quotaService.GetQuotasByDepartmentId(departmentId);
                return Ok(new ApiResponse(StatusCodes.Status200OK, "Department quotas retrieved successfully", quotas));
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
