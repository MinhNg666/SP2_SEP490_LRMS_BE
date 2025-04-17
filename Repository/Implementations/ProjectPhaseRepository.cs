using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Domain.Constants;

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

    public async Task UpdateProjectPhaseStatusesBasedOnDates()
    {
        try
        {
            var today = DateTime.Today;
            Console.WriteLine($"Running automatic status update on {today:yyyy-MM-dd}");
            
            // Debug: Check all project phases
            var allPhases = await _context.ProjectPhases.ToListAsync();
            Console.WriteLine($"Total project phases: {allPhases.Count}");
            foreach (var phase in allPhases)
            {
                Console.WriteLine($"Phase ID: {phase.ProjectPhaseId}, Title: {phase.Title}, " +
                                  $"StartDate: {phase.StartDate:yyyy-MM-dd}, EndDate: {phase.EndDate:yyyy-MM-dd}, " +
                                  $"Current Status: {phase.Status}");
            }
            
            // Find phases that should be "Overdued" (current date after end date, not completed)
            var overduedPhases = await _context.ProjectPhases
                .Where(p => p.EndDate.HasValue && p.EndDate.Value.Date < today && 
                           p.Status != (int)ProjectPhaseStatusEnum.Completed &&
                           p.Status != (int)ProjectPhaseStatusEnum.Overdued)
                .ToListAsync();
            
            Console.WriteLine($"Found {overduedPhases.Count} phases that should be marked as Overdued");
            
            foreach (var phase in overduedPhases)
            {
                Console.WriteLine($"Updating phase {phase.ProjectPhaseId} ({phase.Title}) from status {phase.Status} to Overdued");
                phase.Status = (int)ProjectPhaseStatusEnum.Overdued;
            }
            
            // Find phases that should be "In_progress" (current date between start and end dates)
            var inProgressPhases = await _context.ProjectPhases
                .Where(p => p.StartDate.HasValue && p.EndDate.HasValue && 
                           p.StartDate.Value.Date <= today && 
                           p.EndDate.Value.Date >= today && 
                           p.Status != (int)ProjectPhaseStatusEnum.In_progress &&
                           p.Status != (int)ProjectPhaseStatusEnum.Completed)
                .ToListAsync();
            
            Console.WriteLine($"Found {inProgressPhases.Count} phases that should be marked as In_progress");
            
            foreach (var phase in inProgressPhases)
            {
                Console.WriteLine($"Updating phase {phase.ProjectPhaseId} ({phase.Title}) from status {phase.Status} to In_progress");
                phase.Status = (int)ProjectPhaseStatusEnum.In_progress;
            }
            
            // Find phases that should be "Pending" (current date before start date)
            var pendingPhases = await _context.ProjectPhases
                .Where(p => p.StartDate.HasValue && p.StartDate.Value.Date > today && 
                           p.Status != (int)ProjectPhaseStatusEnum.Pending)
                .ToListAsync();
            
            Console.WriteLine($"Found {pendingPhases.Count} phases that should be marked as Pending");
            
            foreach (var phase in pendingPhases)
            {
                Console.WriteLine($"Updating phase {phase.ProjectPhaseId} ({phase.Title}) from status {phase.Status} to Pending");
                phase.Status = (int)ProjectPhaseStatusEnum.Pending;
            }
            
            // Save all changes
            if (pendingPhases.Any() || inProgressPhases.Any() || overduedPhases.Any())
            {
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {result} changes to the database");
            }
            else
            {
                Console.WriteLine("No status changes needed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateProjectPhaseStatusesBasedOnDates: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    public async Task<bool> UpdateProjectPhaseAsync(int projectPhaseId, int status, decimal spentBudget)
    {
        try
        {
            var projectPhase = await _context.ProjectPhases.FindAsync(projectPhaseId);
            if (projectPhase == null)
                return false;
            
            projectPhase.Status = status;
            projectPhase.SpentBudget = spentBudget;
            _context.ProjectPhases.Update(projectPhase);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating project phase: {ex.Message}");
            return false;
        }
    }
}
