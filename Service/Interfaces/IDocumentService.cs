using Microsoft.AspNetCore.Http;
using Domain.DTO.Responses;
using System.Threading.Tasks;

namespace Service.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponse> SubmitDocument(int projectId, IFormFile file, int documentType, int uploadedBy, int? sequenceId);
    Task<IEnumerable<DocumentResponse>> GetProjectDocuments(int projectId);
    Task<List<DocumentResponse>> SubmitDocuments(int projectId, IEnumerable<IFormFile> files, int documentType, int uploadedBy);
} 