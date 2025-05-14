using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTO.Requests;
using LRMS_API;

namespace Domain.Automapper
{
    public class RequestMappingProfile : Profile
    {
        public RequestMappingProfile() 
        {
            CreateMap<CreateReviewCouncilGroupRequest, Group>();
            CreateMap<CreateNotificationRequest, Notification>();
            CreateMap<CreateStudentGroupRequest, Group>();
            CreateMap<SendInvitationRequest, Invitation>();
            CreateMap<CreateUserRequest, User>();
            CreateMap<ProjectApprovalRequest, Project>();
            CreateMap<CreateProjectRequest, Project>();
            CreateMap<UpdateStudentRequest, User>();
            CreateMap<UpdateUserRequest, User>();
            CreateMap<CreateConferenceFromProjectRequest, Conference>();
            CreateMap<CreateJournalFromProjectRequest, Journal>();
        }
    }
}
