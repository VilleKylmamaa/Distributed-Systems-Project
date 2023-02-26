using DistrLB.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DistrChat.Controllers
{
    public class LoadBalancerController : Controller
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private IHubContext<SignalrHub> HubContext { get; set; }

        public LoadBalancerController(
            IHttpClientFactory httpClientFactory,
            IHubContext<SignalrHub> hubcontext)
        {
            HttpClientFactory = httpClientFactory;
            HubContext = hubcontext;
        }

        [HttpGet]
        public async Task<Uri> GetApplicationServerHost()
        {
            var statusList = new List<ServerStatus>();

            var client1 = HttpClientFactory.CreateClient("ApplicationServer1");
            var client2 = HttpClientFactory.CreateClient("ApplicationServer2");
            var client3 = HttpClientFactory.CreateClient("ApplicationServer3");

            var responses = new List<HttpResponseMessage>
            {
                await client1.GetAsync("Connections"),
                await client2.GetAsync("Connections"),
                await client3.GetAsync("Connections")
            };

            foreach (var response in responses)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<ServerStatus>();
                if (responseContent != null)
                {
                    statusList.Add(responseContent);
                }
            }

            var lowestLoadServer = statusList.Where(status => status.IsAvailable)
                .OrderBy(status => status.ConnectionCount)
                .FirstOrDefault();

            var lowestLoadUrl = new Uri("about:blank");

            if (lowestLoadServer != null)
            {
                lowestLoadUrl = lowestLoadServer.Url;
            }

            var hContextRequest = HttpContext.Request;
            var hostName = hContextRequest.Host.ToString().Replace("host.docker.internal", "localhost");
            var signalrGroupName = "LoadBalancerEvents_" + hostName;
            await HubContext.Clients.Group(signalrGroupName).SendAsync("NewLoadBalancerEvent", new LoadBalancerEvent { Host = hostName, EventMessage = $"Client connected to {lowestLoadUrl}" });

            return lowestLoadUrl;
        }
    }

    public class ServerStatus
    {
        public Uri Url { get; set; } = new Uri("about:blank");
        public bool IsAvailable { get; set; }
        public int ConnectionCount { get; set; }
    }
}
