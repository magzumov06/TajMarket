using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected int UserId 
    {
        get 
        {
            var userIdStr = User.FindFirst("sub")?.Value 
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        
            if (string.IsNullOrEmpty(userIdStr))
                throw new Exception("User ID not found in token. Make sure 'sub' claim exists.");

            return int.Parse(userIdStr);
        }
    }


}