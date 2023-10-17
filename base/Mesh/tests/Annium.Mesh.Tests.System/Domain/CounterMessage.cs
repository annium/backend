using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Tests.System.Domain;

public class CounterMessage : NotificationBaseObsolete
{
    public int Value { get; init; }
}