using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;
public class ProjectRepository: GenericRepository<Project>, IProjectRepository
{
    private readonly LRMSDbContext _context;
    public ProjectRepository(LRMSDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AddDocumentAsync(Document document)
    {
        try
        {
            var projectResource = new ProjectResource
            {
                ResourceName = document.FileName ?? "Unnamed Document",
                ResourceType = 1, // Giả sử 1 là Document type
                Cost = 0, // Optional
                Quantity = 1, // Optional
                Acquired = true, // Optional
                ProjectId = document.ProjectId // Optional nhưng nên có
            };
        
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync(); // Lưu để có ID
        
            // Gán ProjectResourceId cho Document
            document.ProjectResourceId = projectResource.ProjectResourceId;
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi thêm document: {ex.Message}");
        }
    }
    public async Task<int> AddResourceAsync(ProjectResource resource)
    {
        try
        {
            await _context.ProjectResources.AddAsync(resource);
            await _context.SaveChangesAsync();
            return resource.ProjectResourceId;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi thêm project resource: {ex.Message}");
        }
    }
    public async Task<ProjectResource> GetResourceByNameAndProjectId(string resourceName, int projectId)
    {
        return await _context.ProjectResources
            .FirstOrDefaultAsync(r => r.ResourceName == resourceName && r.ProjectId == projectId);
    }
    public async Task<IEnumerable<Project>> GetAllProjectsWithDetailsAsync()
    {
        return await _context.Projects
            .Include(p => p.Documents)
            .Include(p => p.Group)
            .Include(p => p.Department)
            .ToListAsync();
    }
    
    public async Task<Project> GetProjectWithDetailsAsync(int projectId)
    {
        return await _context.Projects
            .Include(p => p.Documents)
            .Include(p => p.Group)
            .Include(p => p.Department)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
    }
    public async Task<int> AddMilestoneAsync(Milestone milestone)
   {
       await _context.Milestones.AddAsync(milestone);
       await _context.SaveChangesAsync();
       return milestone.MilestoneId; // Hoặc ID của milestone nếu có
   }
}
