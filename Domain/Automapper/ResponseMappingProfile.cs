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
            .ForMember(dest => dest.Methodology, opt => opt.MapFrom(src => src.Methodlogy))
            .ForMember(dest => dest.SpentBudget, opt => opt.MapFrom(src => src.SpentBudget));
            
        CreateMap<Document, DocumentResponse>();

        CreateMap<ProjectPhase, ProjectPhaseResponse>()
            .ForMember(dest => dest.SpentBudget, opt => opt.MapFrom(src => src.SpentBudget));

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

        CreateMap<Quota, QuotaResponse>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => 
                src.Project != null ? src.Project.ProjectName : null))
            .ForMember(dest => dest.AllocatorName, opt => opt.MapFrom(src => 
                src.AllocatedByNavigation != null ? src.AllocatedByNavigation.FullName : null));

        CreateMap<FundDisbursement, FundDisbursementResponse>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => 
                Enum.GetName(typeof(FundDisbursementStatusEnum), src.Status ?? 0)))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => 
                src.Project != null ? src.Project.ProjectName : null))
            .ForMember(dest => dest.ProjectPhaseTitle, opt => opt.MapFrom(src => 
                src.ProjectPhase != null ? src.ProjectPhase.Title : null))
            .ForMember(dest => dest.RequesterId, opt => opt.MapFrom(src => src.UserRequest))
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => 
                src.UserRequestNavigation != null ? src.UserRequestNavigation.FullName : null))
            .ForMember(dest => dest.ApprovedById, opt => opt.MapFrom(src => 
                src.AppovedByNavigation != null ? src.AppovedByNavigation.UserId : (int?)null))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => 
                src.AppovedByNavigation != null && src.AppovedByNavigation.User != null ? src.AppovedByNavigation.User.FullName : null))
            .ForMember(dest => dest.DisbursedById, opt => opt.MapFrom(src => src.DisburseBy))
            .ForMember(dest => dest.DisbursedByName, opt => opt.MapFrom(src => 
                src.DisburseByNavigation != null ? src.DisburseByNavigation.FullName : null))
            .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectType : (int?)null))
            .ForMember(dest => dest.ProjectTypeName, opt => opt.MapFrom(src => src.Project != null && src.Project.ProjectType.HasValue ? Enum.GetName(typeof(ProjectTypeEnum), src.Project.ProjectType.Value) : null))
            .ForMember(dest => dest.ProjectApprovedBudget, opt => opt.MapFrom(src => src.Project != null ? src.Project.ApprovedBudget : (decimal?)null))
            .ForMember(dest => dest.ProjectSpentBudget, opt => opt.MapFrom(src => src.Project != null ? src.Project.SpentBudget : 0))
            .ForMember(dest => dest.ProjectDisbursedAmount, opt => opt.MapFrom(src => 
                src.Project != null && src.Project.FundDisbursements != null ?
                src.Project.FundDisbursements
                    .Where(fd => fd.Status == (int)FundDisbursementStatusEnum.Approved || fd.Status == (int)FundDisbursementStatusEnum.Disbursed)
                    .Sum(fd => fd.FundRequest ?? 0) : 0
            ))
            .ForMember(dest => dest.ProjectPhases, opt => opt.MapFrom(src => 
                src.Project != null && src.Project.ProjectPhases != null ?
                src.Project.ProjectPhases.Select(pp => new ProjectPhaseInfo
                {
                    ProjectPhaseId = pp.ProjectPhaseId,
                    Title = pp.Title,
                    StartDate = pp.StartDate,
                    EndDate = pp.EndDate,
                    Status = pp.Status,
                    StatusName = pp.Status.HasValue ? Enum.GetName(typeof(ProjectPhaseStatusEnum), pp.Status.Value) : null,
                    SpentBudget = pp.SpentBudget
                }).ToList() : new List<ProjectPhaseInfo>()
            ));
    } 
}
