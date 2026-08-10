using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirst("sub")?.Value
        ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}