using Annium.Extensions.Arguments;

namespace Demo.Mesh.Client.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "ws";
    public static string Description => "ws toolkit";

    public Group()
    {
        Add<KeepAliveCommand>();
        Add<ListenCommand>();
        Add<RequestResponseCommand>();
        Add<RequestVoidCommand>();
        Add<SubUnsubCommand>();
    }
}