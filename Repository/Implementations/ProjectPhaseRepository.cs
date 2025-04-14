using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;
public class ProjectPhaseRepository : GenericRepository<ProjectPhase>, IProjectPhaseRepository
{
    private readonly LRMSDbContext _context;
    public ProjectPhaseRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<int> AddProjectPhaseAsync(ProjectPhase projectPhase)
    {
        await _context.ProjectPhases.AddAsync(projectPhase);
        await _context.SaveChangesAsync();
        return projectPhase.ProjectPhaseId;
    }

    public async Task<bool> UpdateProjectPhaseStatusAsync(int projectPhaseId, int status)
    {
        try
        {
            var projectPhase = await _context.ProjectPhases.FindAsync(projectPhaseId);
            if (projectPhase == null)
                return false;
            
            projectPhase.Status = status;
            _context.ProjectPhases.Update(projectPhase);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating project phase status: {ex.Message}");
            return false;
        }
    }
}
