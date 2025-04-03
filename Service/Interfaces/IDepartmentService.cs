using Domain.DTO.Responses;

namespace Service.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponse>> GetAllDepartments();
}
