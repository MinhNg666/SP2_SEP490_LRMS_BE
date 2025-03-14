using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Interfaces;

namespace LRMS_API.Controllers;

[ApiController]
public class PublicationController : ApiBaseController
{
    private readonly IPublicationService _publicationService;

    public PublicationController(IPublicationService publicationService)
    {
        _publicationService = publicationService;
    }

    [HttpGet("publications")]
    public async Task<IActionResult> GetAllPublications()
    {
        try
        {
            var result = await _publicationService.GetAllPublications();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }




    [HttpGet("publications/{id}")]
    public async Task<IActionResult> GetPublicationById(int id)
    {
        try
        {
            var result = await _publicationService.GetPublicationById(id);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
} 