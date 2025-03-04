using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses
{
    public class StudentProfileResponse
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public int? GroupId { get; set; }
    }
}
