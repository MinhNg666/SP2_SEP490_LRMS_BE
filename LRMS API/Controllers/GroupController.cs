﻿using System.Security.Claims;
using Domain.Constants;
using Domain.DTO.Common;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Implementations;
using Service.Interfaces;

namespace LRMS_API.Controllers;
[ApiController]

public class GroupController : ApiBaseController
{
    private readonly IGroupService _groupService;
    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }
    [HttpGet("groups")]
    public async Task<IActionResult> GetAllGroups() 
    {
        try
        {
            var result = await _groupService.GetAllGroups();
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));

        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpGet("groups/{groupid}")]
    public async Task<IActionResult> GetGroupById( int groupid)
    {
        try
        {
            var result = await _groupService.GetGroupById(groupid);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL, result));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("council-groups")]
    public async Task<IActionResult> CreateCouncilGroup(CreateCouncilGroupRequest request) 
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Lấy ID người dùng từ Claims
            await _groupService.CreateCouncilGroup(request, currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
    [HttpPost("student-groups")]
    public async Task<IActionResult> CreateStudentGroup(CreateStudentGroupRequest request) 
    {
        try 
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _groupService.CreateStudentGroup(request, currentUserId);
            return Ok(new ApiResponse(StatusCodes.Status200OK, MessageConstants.SUCCESSFUL));
        }
        catch (ServiceException e)
        {
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, e.Message));
        }
    }
}
