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
            .ForMember(dest => dest.UserId, opt =>
                opt.MapFrom(src => src.UserId))
            .ReverseMap();
        CreateMap<Group, GroupResponse>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.GroupMembers));

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
    } 
}
