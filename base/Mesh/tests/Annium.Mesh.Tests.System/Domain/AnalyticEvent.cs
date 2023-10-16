using Annium.Mesh.Domain.Requests;

namespace Annium.Mesh.Tests.System.Domain;

public class AnalyticEvent : EventBase
{
    public string Message { get; }

    public AnalyticEvent(string message)
    {
        Message = message;
    }
}