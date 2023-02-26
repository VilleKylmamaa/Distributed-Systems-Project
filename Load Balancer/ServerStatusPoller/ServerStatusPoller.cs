using DistrChat.Controllers;
using DistrLB.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace DistrLB.ServerStatusPoller
{
    public class ServerStatusPoller : BackgroundService
    {
        private readonly PeriodicTimer timer = new(TimeSpan.FromMilliseconds(1000));
        private readonly IHttpClientFactory HttpClientFactory;
        private IHubContext<SignalrHub> HubContext { get; set; }

        public ServerStatusPoller(
            IHttpClientFactory httpClientFactory,
            IHubContext<SignalrHub> hubcontext)
        {
            HttpClientFactory = httpClientFactory;
            HubContext = hubcontext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (await timer.WaitForNextTickAsync(cancellationToken)
                   && !cancellationToken.IsCancellationRequested)
            {
                await CallServerStatus();
                await Task.Delay(1000, cancellationToken);
            }
        }

        private async Task CallServerStatus()
        {
            var statusList = new List<ServerStatus>();
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
                    var responseContent = await response.Content.ReadFromJsonAsync<ServerStatus>();
                    if (responseContent != null)
                    {
                        statusList.Add(responseContent);
                    }
                }
                catch
                {
                    // Server is unavailable
                    var urlLocal = appServerClient.BaseAddress?.ToString().Replace("host.docker.internal", "localhost");
                    var url = urlLocal != null && urlLocal != string.Empty
                        ? new Uri(urlLocal + "SignalrHub")
                        : new Uri("about:blank");

                    statusList.Add(new ServerStatus
                    {
                        Url = url,
                        IsAvailable = false,
                        ConnectionCount = 0,
                    });
                }
            }

            await HubContext.Clients.All.SendAsync("NewServerStatusReport", new { statusList });
        }
    }
}
