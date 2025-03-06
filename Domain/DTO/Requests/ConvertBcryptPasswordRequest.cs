using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Requests;

public class ConvertBcryptPasswordRequest
{
    public string Password { get; set; }
}
