using Microsoft.AspNetCore.SignalR;

namespace TeamsApplicationServer.WebSockets
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name ?? connection.ConnectionId;
        }
    }

}
