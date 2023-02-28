using DistrChat.SignalR;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistrChat.Controllers
{
    public class ConnectionsController : Controller
    {
        readonly SignalrHub SignalrHub;
        private readonly IConnectionMultiplexer RedisConnection;

        public ConnectionsController(SignalrHub signalrHub, IConnectionMultiplexer redisConnection)
        {
            SignalrHub = signalrHub;
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

            var isRedisConnectionAlive = RedisConnection.IsConnected;

            var response = new ServerStatus
            {
                Url = baseUrl,
                IsAvailable = isRedisConnectionAlive,
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
