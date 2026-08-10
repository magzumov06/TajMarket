using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

[Authorize]  
public class NotificationHub : Hub;
