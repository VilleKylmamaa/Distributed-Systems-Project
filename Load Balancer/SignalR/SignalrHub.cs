using Microsoft.AspNetCore.SignalR;

namespace DistrLB.SignalR
{
    public class SignalrHub : Hub
    {
        public async Task SubscribeToLoadBalancerEvents()
        {
            var host = Context.GetHttpContext()?.Request.Host;
            var groupName = "LoadBalancerEvents_" + host;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task NewLoadBalancerEvent(LoadBalancerEvent lbEvent)
        {
            var groupName = "LoadBalancerEvents_" + lbEvent.Host;

            await Clients.Group(groupName).SendAsync(
                "NewLoadBalancerEvent",
                lbEvent.EventMessage
            );
        }
    }
}
