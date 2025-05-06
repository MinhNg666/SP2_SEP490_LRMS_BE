using Domain.DTO.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IQuotaService
    {
        Task<IEnumerable<QuotaResponse>> GetAllQuotas();
        Task<IEnumerable<QuotaResponse>> GetQuotasByUserId(int userId);
        Task<QuotaDetailResponse> GetQuotaDetailById(int quotaId);
    }
}
