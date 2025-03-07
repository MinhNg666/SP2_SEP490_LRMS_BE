using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests
{
    public class UpdateUserRequest
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public int? RoleId { get; set; }

        public int? DepartmentId { get; set; }

        public int? Status { get; set; }

        public int? GroupId { get; set; }

        public string? Level { get; set; }
    }
}
