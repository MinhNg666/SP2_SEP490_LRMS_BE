
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
    } 
}
