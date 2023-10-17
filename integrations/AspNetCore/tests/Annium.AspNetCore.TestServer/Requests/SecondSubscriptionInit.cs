using Annium.Mesh.Domain.Requests;

namespace Annium.AspNetCore.TestServer.Requests;

public class SecondSubscriptionInit : SubscriptionInitRequestBaseObsolete
{
    public string Param { get; set; } = string.Empty;
}