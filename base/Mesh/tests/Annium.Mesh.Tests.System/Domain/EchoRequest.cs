using Annium.Mesh.Domain;

namespace Annium.Mesh.Tests.System.Domain;

public class EchoRequest : RequestBase
{
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}