namespace Domain.DTO.Responses;
public class UserGroupResponse
{
    public int? GroupId { get; set; }
    public string GroupName { get; set; }
    public int? Role { get; set; }
    public int? GroupType { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; }
}
