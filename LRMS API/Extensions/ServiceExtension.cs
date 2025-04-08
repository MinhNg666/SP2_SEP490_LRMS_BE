using Repository.Implementations;
using Repository.Interfaces;
using Service.Settings;
using Service.Interfaces;
using Service.Implementations;
using Microsoft.EntityFrameworkCore;

namespace LRMS_API.Extensions;
public static class ServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddDbContext<LRMSDbContext>(options =>
    options.UseSqlServer(connectionString));


        services.Configure<AdminAccount>(config.GetSection("AdminAccount"));
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        //Service
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBcryptPasswordService, BcryptPasswordService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITimelineService, TimelineService>();
        services.AddScoped<ITimelineSequenceService, TimelineSequenceService>();
        services.AddScoped<IDocumentService, DocumentService>();
        //services.AddScoped<IPublicationService, PublicationService>();
        
        //Repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ITimelineSequenceRepository, TimelineSequenceRepository>();
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }
}
