using Microsoft.AspNetCore.Mvc;

namespace DistrChat.Controllers
{
    public class LoadBalancerController : Controller
    {
        private readonly IHttpClientFactory HttpClientFactory;

        public LoadBalancerController(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<Uri> GetApplicationServerHost()
        {
            var statusList = new List<ServerStatus>();

            var client1 = HttpClientFactory.CreateClient("ApplicationServer1");
            var client2 = HttpClientFactory.CreateClient("ApplicationServer2");

            var response1 = await client1.GetAsync("Connections");
            var response2 = await client2.GetAsync("Connections");

            if (response1.IsSuccessStatusCode)
            {
                var responseContent = await response1.Content.ReadFromJsonAsync<ServerStatus>();
                if (responseContent != null)
                {
                    statusList.Add(responseContent);
                }
            }

            if (response2.IsSuccessStatusCode)
            {
                var responseContent = await response2.Content.ReadFromJsonAsync<ServerStatus>();
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
