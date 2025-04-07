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
}
