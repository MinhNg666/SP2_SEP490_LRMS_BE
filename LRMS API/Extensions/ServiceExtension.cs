﻿using Repository.Implementations;
using Repository.Interfaces;
using Service.Settings;
using Service.Interfaces;
using Service.Implementations;

namespace LRMS_API.Extensions;
public static class ServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.Configure<AdminAccount>(config.GetSection("AdminAccount"));
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBcryptPasswordService, BcryptPasswordService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IInvitationService, InvitationService>();

        return services;
    }
}
