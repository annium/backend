using Annium.Mesh.Domain.Requests;

namespace Annium.AspNetCore.TestServer.Requests;

public class EchoRequest : RequestBaseObsolete
{
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}