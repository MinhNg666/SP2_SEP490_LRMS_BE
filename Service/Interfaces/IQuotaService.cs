using Domain.DTO.Requests;
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
        Task<int> AllocateQuotaToDepartment(AllocateDepartmentQuotaRequest request, int allocatedBy);
        Task<IEnumerable<QuotaResponse>> GetDepartmentQuotas();
        Task<IEnumerable<QuotaResponse>> GetQuotasByDepartmentId(int departmentId);
        Task<decimal> GetAvailableDepartmentBudget(int departmentId);
        Task<int> CreateProjectQuota(int projectId, decimal amount, int departmentId, int allocatedBy);
    }
}
