using DistrChat.SignalR;
using Microsoft.AspNetCore.Mvc;

namespace DistrChat.Controllers
{
    public class ConnectionsController : Controller
    {
        SignalrHub SignalrHub;

        public ConnectionsController(SignalrHub signalrHub)
        {
            SignalrHub = signalrHub;
        }

        [HttpGet]
        public ServerStatus Connections()
        {
            var roomClientCounts = SignalrHub.GetRoomClientCounts();

            int totalConnectionCount = 0;
            foreach (var roomClientCount in roomClientCounts)
            {
                totalConnectionCount += roomClientCount.Value;
            }

            var hContextRequest = HttpContext.Request;
            var hostName = hContextRequest.Host.ToString().Replace("host.docker.internal", "localhost");
            var baseUrl = $"{hContextRequest.Scheme}://{hostName}/SignalrHub";

            var response = new ServerStatus
            {
                Url = baseUrl,
                IsAvailable = true,
                ConnectionCount = totalConnectionCount
            };

            return response;
        }
    }

    public class ServerStatus
    {
        public string Url { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int ConnectionCount { get; set; }
    }
}
