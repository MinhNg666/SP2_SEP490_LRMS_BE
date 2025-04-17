using LRMS_API;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interfaces;
public interface IFundDisbursementRepository : IGenericRepository<FundDisbursement>
{
    Task<IEnumerable<FundDisbursement>> GetAllWithDetailsAsync();
    Task<FundDisbursement> GetByIdWithDetailsAsync(int fundDisbursementId);
    Task<IEnumerable<FundDisbursement>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<FundDisbursement>> GetByUserIdAsync(int userId);
}
