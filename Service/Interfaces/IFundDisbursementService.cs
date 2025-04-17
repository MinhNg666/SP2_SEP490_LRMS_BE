using Domain.DTO.Requests;
using Domain.DTO.Responses;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces;
public interface IFundDisbursementService
{
    Task<int> CreateFundDisbursementRequest(CreateFundDisbursementRequest request, int userId);
    Task<IEnumerable<FundDisbursementResponse>> GetAllFundDisbursements();
    Task<FundDisbursementResponse> GetFundDisbursementById(int fundDisbursementId);
    Task<IEnumerable<FundDisbursementResponse>> GetFundDisbursementsByProjectId(int projectId);
    Task<IEnumerable<FundDisbursementResponse>> GetFundDisbursementsByUserId(int userId);
    Task<bool> UploadDisbursementDocument(int fundDisbursementId, IFormFile documentFile, int userId);
    Task<bool> UploadDisbursementDocuments(int fundDisbursementId, IEnumerable<IFormFile> documentFiles, int userId);
    Task<bool> ApproveFundDisbursement(int fundDisbursementId, string approvalComments, int approverId);
    Task<bool> RejectFundDisbursement(int fundDisbursementId, string rejectionReason, int rejectorId);
}
