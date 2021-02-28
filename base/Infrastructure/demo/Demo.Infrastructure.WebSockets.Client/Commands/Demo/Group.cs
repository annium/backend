namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "demo";
        public override string Description { get; } = "demo commands";

        public Group()
        {
            // commands
            Add<RequestCommand>();
            Add<EchoCommand>();
            Add<SinkCommand>();
        }
    }
}