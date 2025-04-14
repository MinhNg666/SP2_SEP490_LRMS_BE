using AutoMapper;
using Domain.DTO.Common;
using Domain.DTO.Responses;
using Domain.Constants;
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
            .ForMember(dest => dest.ProjectPhases, opt => opt.MapFrom(src => src.ProjectPhases))
            .ForMember(dest => dest.Methodology, opt => opt.MapFrom(src => src.Methodlogy));
            
        CreateMap<Document, DocumentResponse>();
        CreateMap<ProjectPhase, ProjectPhaseResponse>();
        CreateMap<Journal, JournalResponse>()
            .ForMember(dest => dest.ProjectName, opt => 
                opt.MapFrom(src => src.Project == null ? string.Empty : src.Project.ProjectName))
            .ForMember(dest => dest.Document, opt => 
                opt.MapFrom(src => (src.Project == null || src.Project.Documents == null) ? null : 
                    src.Project.Documents.FirstOrDefault(d => d.DocumentType == (int)DocumentTypeEnum.JournalPaper)));

        CreateMap<Conference, ConferenceResponse>()
            .ForMember(dest => dest.ProjectName, opt => 
                opt.MapFrom(src => src.Project == null ? string.Empty : src.Project.ProjectName))
            .ForMember(dest => dest.Expense, opt => 
                opt.MapFrom(src => src.ConferenceExpenses == null ? null : src.ConferenceExpenses.FirstOrDefault()))
            .ForMember(dest => dest.Documents, opt => 
                opt.MapFrom(src => (src.ConferenceExpenses == null || !src.ConferenceExpenses.Any() || 
                    src.ConferenceExpenses.FirstOrDefault() == null || 
                    src.ConferenceExpenses.FirstOrDefault().Documents == null) ? 
                    new List<Document>() : src.ConferenceExpenses.FirstOrDefault().Documents));

        CreateMap<ConferenceExpense, ConferenceExpenseResponse>();
    } 
}
