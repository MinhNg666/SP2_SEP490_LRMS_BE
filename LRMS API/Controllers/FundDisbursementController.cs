using System.Security.Claims;
using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Domain.Constants;

namespace LRMS_API.Controllers;
[ApiController]
public class FundDisbursementController : ApiBaseController
{
    private readonly IFundDisbursementService _fundDisbursementService;
    
    public FundDisbursementController(IFundDisbursementService fundDisbursementService)
    {
        _fundDisbursementService = fundDisbursementService;
    }
    
    [HttpPost("fund-disbursements")]
    [Authorize]
    public async Task<IActionResult> CreateFundDisbursementRequest([FromBody] CreateFundDisbursementRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var disbursementId = await _fundDisbursementService.CreateFundDisbursementRequest(request, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement request created successfully", new { DisbursementId = disbursementId }));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpGet("fund-disbursements")]
    [Authorize(Roles = "Admin,Lecturer,Office")]
    public async Task<IActionResult> GetAllFundDisbursements()
    {
        try
        {
            var disbursements = await _fundDisbursementService.GetAllFundDisbursements();
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursements retrieved successfully", disbursements));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpGet("fund-disbursements/{disbursementId}")]
    [Authorize]
    public async Task<IActionResult> GetFundDisbursementById(int disbursementId)
    {
        try
        {
            var disbursement = await _fundDisbursementService.GetFundDisbursementById(disbursementId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement retrieved successfully", disbursement));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpGet("projects/{projectId}/fund-disbursements")]
    [Authorize]
    public async Task<IActionResult> GetFundDisbursementsByProject(int projectId)
    {
        try
        {
            var disbursements = await _fundDisbursementService.GetFundDisbursementsByProjectId(projectId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursements retrieved successfully", disbursements));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpGet("my-fund-disbursements")]
    [Authorize]
    public async Task<IActionResult> GetMyFundDisbursements()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var disbursements = await _fundDisbursementService.GetFundDisbursementsByUserId(userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Your fund disbursements retrieved successfully", disbursements));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpPost("fund-disbursements/{disbursementId}/upload-document")]
    [Authorize]
    public async Task<IActionResult> UploadDisbursementDocuments(int disbursementId, List<IFormFile> documentFiles)
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
            await _fundDisbursementService.UploadDisbursementDocuments(disbursementId, documentFiles, userId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Documents uploaded successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpPost("fund-disbursements/{disbursementId}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveFundDisbursement(
        int disbursementId, 
        [FromForm] ApproveFundDisbursementRequest request,
        List<IFormFile> documentFiles)
    {
        try
        {
            if (disbursementId != request.FundDisbursementId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Disbursement IDs do not match"));
            
            if (documentFiles == null || !documentFiles.Any())
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Please attach confirmation documents"));
            
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            foreach (var file in documentFiles)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, $"Only PDF, DOC, and DOCX files are allowed. Invalid file: {file.FileName}"));
            }
                
            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                    
            var result = await _fundDisbursementService.ApproveFundDisbursement(
                disbursementId, 
                request.ApprovalComments ?? "", 
                approverId,
                documentFiles);
                
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement approved successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
    
    [HttpPost("fund-disbursements/{disbursementId}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectFundDisbursement(
        int disbursementId, 
        [FromForm] RejectFundDisbursementRequest request,
        List<IFormFile> documentFiles)
    {
        try
        {
            if (disbursementId != request.FundDisbursementId)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Disbursement IDs do not match"));
                
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
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
                
            var rejectorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                    
            var result = await _fundDisbursementService.RejectFundDisbursement(
                disbursementId, 
                request.RejectionReason, 
                rejectorId,
                documentFiles);
                
            return Ok(new ApiResponse(StatusCodes.Status200OK, "Fund disbursement rejected successfully"));
        }
        catch (ServiceException ex)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
