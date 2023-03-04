using DistrChat.Controllers;
using DistrChat.SignalR;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace DistrLB.RedisStatusPoller
{
    public class RedisStatusPoller : BackgroundService
    {
        private readonly PeriodicTimer timer = new(TimeSpan.FromMilliseconds(1000));
        private IHubContext<SignalrHub> HubContext { get; set; }
        private readonly IConnectionMultiplexer RedisConnection;

        public RedisStatusPoller(
            IHubContext<SignalrHub> hubcontext,
            IConnectionMultiplexer redisConnection)
        {
            HubContext = hubcontext;
            RedisConnection = redisConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (await timer.WaitForNextTickAsync(cancellationToken)
                   && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateRedisConnectionStatusToUi();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(1000, cancellationToken);
            }
        }

        /// <summary>
        /// Checks each Redis endpoint connection and updates their state to the app server UI
        /// </summary>
        private async Task UpdateRedisConnectionStatusToUi()
        {
            try
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
                await HubContext.Clients.Group("RedisStatusUpdates").SendAsync("RedisStatusUpdate", redisConnections);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
