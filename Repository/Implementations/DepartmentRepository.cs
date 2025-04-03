using LRMS_API;
using Repository.Interfaces;

namespace Repository.Implementations;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    private readonly LRMSDbContext _context;

    public DepartmentRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
    
    // We'll use the base implementation from GenericRepository
}