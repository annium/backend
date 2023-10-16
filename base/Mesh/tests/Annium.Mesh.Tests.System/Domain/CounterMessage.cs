using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Tests.System.Domain;

public class CounterMessage : NotificationBase
{
    public int Value { get; init; }
}