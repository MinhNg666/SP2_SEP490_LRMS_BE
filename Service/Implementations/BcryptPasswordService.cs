using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Service.Interfaces;

namespace Service.Implementations;

public class BcryptPasswordService : IBcryptPasswordService
{
    public async Task<string> ChangePassword(ConvertBcryptPasswordRequest request)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        return hashedPassword;
    }
}
