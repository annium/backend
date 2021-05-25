using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.AspNetCore.TestServer.Requests
{
    public class SecondSubscriptionInit : SubscriptionInitRequestBase
    {
        public string Param { get; set; } = string.Empty;
    }
}