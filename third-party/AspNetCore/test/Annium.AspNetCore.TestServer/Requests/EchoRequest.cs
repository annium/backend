using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.AspNetCore.TestServer.Requests;

public class EchoRequest : RequestBase
{
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}