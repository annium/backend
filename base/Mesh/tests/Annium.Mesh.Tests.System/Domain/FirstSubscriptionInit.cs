using Annium.Mesh.Domain.Requests;

namespace Annium.Mesh.Tests.System.Domain;

public class FirstSubscriptionInit : SubscriptionInitRequestBase
{
    public string Param { get; set; } = string.Empty;
}