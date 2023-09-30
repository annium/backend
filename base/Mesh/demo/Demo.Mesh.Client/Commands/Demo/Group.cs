using Annium.Extensions.Arguments;

namespace Demo.Mesh.Client.Commands.Demo;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "demo";
    public static string Description => "demo commands";

    public Group()
    {
        // commands
        Add<EchoCommand>();
        Add<RequestCommand>();
        Add<SinkCommand>();
    }
}