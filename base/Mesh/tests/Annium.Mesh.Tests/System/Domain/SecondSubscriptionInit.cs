using Annium.Mesh.Domain.Requests;

namespace Annium.Mesh.Tests.System.Domain;

public class SecondSubscriptionInit : SubscriptionInitRequestBase
{
    public string Param { get; set; } = string.Empty;
}