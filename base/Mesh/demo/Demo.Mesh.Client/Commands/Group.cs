using Annium.Extensions.Arguments;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "ws";
    public static string Description => "ws toolkit";

    public Group()
    {
        Add<Demo.Group>();
        Add<KeepAliveCommand>();
        Add<ListenCommand>();
        Add<RequestResponseCommand>();
        Add<RequestVoidCommand>();
        Add<SubUnsubCommand>();
    }
}