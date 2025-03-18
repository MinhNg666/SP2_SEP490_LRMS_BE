using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;
public class ProjectRepository: GenericRepository<Project>, IProjectRepository
{
    private readonly LRMSDbContext _context;
    public ProjectRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
}
