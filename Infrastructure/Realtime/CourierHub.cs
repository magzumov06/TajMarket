using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

[Authorize(Roles = "Admin,Seller,Courier")]
public class CourierHub : Hub
{
    
}