using LRMS_API;
using Repository.Implementations;

namespace Repository.Interfaces;
public interface IProjectRepository: IGenericRepository<Project>
{
    Task<int> AddMilestoneAsync(Milestone milestone);
    Task AddDocumentAsync(Document document);
    Task<int> AddResourceAsync(ProjectResource resource);
    Task<ProjectResource> GetResourceByNameAndProjectId(string resourceName, int projectId);
    Task<IEnumerable<Project>> GetAllProjectsWithDetailsAsync();
    Task<Project> GetProjectWithDetailsAsync(int projectId);
}
