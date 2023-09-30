using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.User;

public class UserActionNotification : EventBase
{
    public string Data { get; set; } = string.Empty;
}