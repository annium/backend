namespace Demo.Logging.Commands;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "";
    public override string Description { get; } = "log toolkit";

    public Group()
    {
        Add<ConsoleCommand>();
        Add<InMemoryCommand>();
        Add<SeqCommand>();
    }
}