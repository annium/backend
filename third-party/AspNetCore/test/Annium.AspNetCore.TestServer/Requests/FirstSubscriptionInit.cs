using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.AspNetCore.TestServer.Requests
{
    public class FirstSubscriptionInit : SubscriptionInitRequestBase
    {
        public string Param { get; set; } = string.Empty;
    }
}