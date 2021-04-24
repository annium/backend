namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "ws";
        public override string Description { get; } = "ws toolkit";

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
}