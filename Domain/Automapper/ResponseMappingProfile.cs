using AutoMapper;
using Domain.DTO.Common;
using Domain.DTO.Responses;
using LRMS_API;

namespace Domain.Automapper;

public class ResponseMappingProfile : Profile
{
    public ResponseMappingProfile()
    {
        CreateMap<User, UserResponse>().ReverseMap();
        CreateMap<User, LoginResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => ""))
            .ForMember(dest => dest.TokenExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<Group, GroupResponse>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.GroupMembers))
            .ForMember(dest => dest.DepartmentName, opt => 
                opt.MapFrom(src => src.GroupDepartmentNavigation != null ? 
                    src.GroupDepartmentNavigation.DepartmentName : null));

        CreateMap<GroupMember, GroupMemberResponse>()
            .ForMember(dest => dest.MemberName, opt => 
                opt.MapFrom(src => src.User != null ? src.User.Username : "Unknown"))
            .ForMember(dest => dest.MemberEmail, opt => 
                opt.MapFrom(src => src.User != null ? src.User.Email : "Unknown"))
            .ForMember(dest => dest.UserId, opt => 
                opt.MapFrom(src => src.UserId));
        CreateMap<Notification, NotificationResponse>().ReverseMap();
        CreateMap<Invitation, InvitationResponse>().ReverseMap();
        CreateMap<User, StudentProfileResponse>().ReverseMap();
        CreateMap<GroupMember, UserGroupResponse>().ReverseMap();
        CreateMap<Notification, NotificationResponse>().ReverseMap();
        CreateMap<User, StudentResponse>().ReverseMap();
        CreateMap<User, LecturerResponse>().ReverseMap();
        CreateMap<Department, DepartmentResponse>();
        CreateMap<Project, ProjectResponse>()
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group.GroupName))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Department.DepartmentId))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
            .ForMember(dest => dest.Milestones, opt => opt.MapFrom(src => src.Milestones))
            .ForMember(dest => dest.Methodology, opt => opt.MapFrom(src => src.Methodlogy));
            
        CreateMap<Document, DocumentResponse>();
        CreateMap<Milestone, MilestoneResponse>();
    } 
}
