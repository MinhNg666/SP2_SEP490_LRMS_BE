using Domain.Constants;
using Domain.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace LRMS_API.Controllers;

public class DepartmentController : ApiBaseController
{
    private readonly IDepartmentService _departmentService;
    
    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }
    
    [HttpGet("all-departments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllDepartments()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartments();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, departments));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
