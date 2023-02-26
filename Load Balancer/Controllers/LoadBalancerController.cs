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
            Console.WriteLine("Connecting user to an application server...");
            var statusList = new List<ServerStatus>();
            var responses = new List<HttpResponseMessage>();
            var appServerClients = new List<HttpClient>()
            {
                HttpClientFactory.CreateClient("ApplicationServer1"),
                HttpClientFactory.CreateClient("ApplicationServer2"),
                HttpClientFactory.CreateClient("ApplicationServer3")
            };

            foreach (var appServerClient in appServerClients)
            {
                try
                {
                    var response = await appServerClient.GetAsync("Connections");
                    responses.Add(response);
                }
                catch
                {
                    statusList.Add(new ServerStatus
                    {
                        Url = appServerClient.BaseAddress,
                        IsAvailable = false,
                        ConnectionCount = 0,
                    });
                }
            }

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
            if (lowestLoadServer?.Url != null)
            {
                lowestLoadUrl = lowestLoadServer.Url;
            }

            var hContextRequest = HttpContext.Request;
            var hostName = hContextRequest.Host.ToString().Replace("host.docker.internal", "localhost");
            var signalrGroupName = "LoadBalancerEvents_" + hostName;

            if (!lowestLoadUrl.Equals(new Uri("about:blank")))
            {
                await HubContext.Clients.Group(signalrGroupName).SendAsync(
                    "NewLoadBalancerEvent", new LoadBalancerEvent { Host = hostName, EventMessage = $"Client connected to {lowestLoadUrl}" }
                );
                Console.WriteLine($"Connected user to {lowestLoadUrl}");
            }
            else
            {
                await HubContext.Clients.Group(signalrGroupName).SendAsync(
                    "NewLoadBalancerEvent", new LoadBalancerEvent { Host = hostName, EventMessage = $"Client attempted to connect while all app servers were unavailable" }
                );
                Console.WriteLine("All application servers were unavailable. Failed to connect user.");
            }

            return lowestLoadUrl;
        }
    }

    public class ServerStatus
    {
        public Uri? Url { get; set; }
        public bool IsAvailable { get; set; }
        public int ConnectionCount { get; set; }
    }
}
