using Microsoft.AspNetCore.Mvc;

namespace LRMS_API.Controllers;
[ApiController]
[Route("api/")]

public class ApiBaseController : ControllerBase
{
    protected int GetAuthorizedAccountId()
    {
        try
        {
            var accountId = int.Parse(User.FindFirst("AccountId")?.Value);
            return accountId;
        }
        catch (Exception e)
        {
            return 0;
        }
    }
}
