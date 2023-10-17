using Annium.Mesh.Domain.Requests;

namespace Annium.Mesh.Tests.System.Domain;

public class EchoRequest : RequestBaseObsolete
{
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}