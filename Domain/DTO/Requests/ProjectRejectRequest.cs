using Microsoft.AspNetCore.Http;

namespace Domain.DTO.Requests;
public class ProjectRejectRequest
{
    public int ProjectId { get; set; }
    public int CouncilGroupId { get; set; }
    public IFormFile DocumentFile { get; set; }
}
