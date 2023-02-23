namespace DistrLB.SignalR
{
    public class LoadBalancerEvent
    {
        public string Host { get; set; } = string.Empty;
        public string EventMessage { get; set; } = string.Empty;
    }
}
