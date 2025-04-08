using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.DTO.Responses;

namespace Service.Interfaces;

public interface IPublicationService
{
    Task<IEnumerable<PublicationResponse>> GetAllPublications();
    Task<PublicationResponse> GetPublicationById(int id);
} 