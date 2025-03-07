using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Requests;

namespace Service.Interfaces;

public interface IBcryptPasswordService
{
        Task<string> ChangePassword(ConvertBcryptPasswordRequest request);

}
