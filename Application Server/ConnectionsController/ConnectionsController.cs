using DistrChat.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace DistrChat.Controllers
{
    public class ConnectionsController : Controller
    {
        readonly SignalrHub SignalrHub;
        private readonly IHubContext<SignalrHub> HubContext;
        private readonly IConnectionMultiplexer RedisConnection;

        public ConnectionsController(
            SignalrHub signalrHub,
            IHubContext<SignalrHub> hubContext,
            IConnectionMultiplexer redisConnection)
        {
            SignalrHub = signalrHub;
            HubContext = hubContext;
            RedisConnection = redisConnection;
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
                IsAvailable = RedisConnection.IsConnected,
                ConnectionCount = totalConnectionCount
            };

            return response;
        }

        private void UpdateRedisConnectionStatusToUi()
        {
            var endPoints = RedisConnection.GetEndPoints();
            var redisConnections = new List<RedisConnection>();
            foreach (var endPoint in endPoints)
            {
                var server = RedisConnection.GetServer(endPoint);
                var redisConnection = new RedisConnection()
                {
                    Url = endPoint.ToString(),
                    IsConnected = server.IsConnected
                };
                redisConnections.Add(redisConnection);
            }
            HubContext.Clients.All.SendAsync("RedisStatusUpdate", redisConnections);
        }
    }

    public class ServerStatus
    {
        public string Url { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int ConnectionCount { get; set; }
    }

    public class RedisConnection
    {
        public string Url { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
    }
}
