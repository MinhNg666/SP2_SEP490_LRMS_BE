using System.Collections.Generic;

namespace Domain.DTO.Responses
{
    public class ProjectDetailResponse : ProjectResponse
    {
        public UserShortInfo CreatedByUser { get; set; }
        public UserShortInfo ApprovedByUser { get; set; }
        public GroupDetailInfo Group { get; set; }
        public DepartmentResponse Department { get; set; }
    }

    public class UserShortInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class GroupDetailInfo
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int GroupType { get; set; }
        public int CurrentMember { get; set; }
        public int MaxMember { get; set; }
        public int? GroupDepartment { get; set; }
        public string DepartmentName { get; set; }
        public IEnumerable<GroupMemberResponse> Members { get; set; }
    }
}
