using MessagePack;

namespace Annium.Mesh.Tests.System.Domain;

[MessagePackObject]
public class EchoRequest
{
    [Key(0)]
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}
