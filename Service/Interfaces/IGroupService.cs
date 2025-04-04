﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;
public interface IGroupService
{
    Task<IEnumerable<GroupResponse>> GetAllGroups();
    Task<GroupResponse> GetGroupById(int groupId);
    Task<IEnumerable<GroupResponse>> GetGroupsByUserId(int userId);
    Task CreateStudentGroup(CreateStudentGroupRequest request, int currentUserId);
    Task CreateCouncilGroup(CreateCouncilGroupRequest request, int currentUserId);
}
