using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.User
{
    public class UserActionNotification : RequestBase
    {
        public string Data { get; set; } = string.Empty;
    }
}