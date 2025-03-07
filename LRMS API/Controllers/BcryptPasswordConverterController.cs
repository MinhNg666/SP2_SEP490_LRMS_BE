using Domain.DTO.Requests;
using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Interfaces;


namespace LRMS_API.Controllers;
[ApiController]

public class BcryptPasswordConverterController : ApiBaseController
{
    private readonly IBcryptPasswordService _bcryptPasswordService;

    public BcryptPasswordConverterController(IBcryptPasswordService bcryptPasswordService)
    {
        _bcryptPasswordService = bcryptPasswordService;
    }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ConvertBcryptPasswordRequest request)
    {
        var hashedPassword = await _bcryptPasswordService.ChangePassword(request);
        return Ok(new { HashedPassword = hashedPassword });
    }
}
