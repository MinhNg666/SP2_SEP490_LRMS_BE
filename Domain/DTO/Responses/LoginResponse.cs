using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int Status { get; set; }
        public int Role { get; set; }
        public string AccessToken { get; set; }
    }
}
