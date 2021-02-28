using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.User
{
    public record UserActionNotification : RequestBase
    {
        public string Data { get; set; } = string.Empty;
    }
}