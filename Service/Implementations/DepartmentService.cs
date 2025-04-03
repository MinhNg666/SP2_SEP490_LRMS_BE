using AutoMapper;
using Domain.DTO.Responses;
using LRMS_API;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;
    
    public DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<DepartmentResponse>> GetAllDepartments()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<DepartmentResponse>>(departments);
    }
}
