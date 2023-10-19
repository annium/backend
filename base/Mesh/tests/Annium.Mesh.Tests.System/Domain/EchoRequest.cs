namespace Annium.Mesh.Tests.System.Domain;

public class EchoRequest
{
    public string Message { get; }

    public EchoRequest(string message)
    {
        Message = message;
    }
}