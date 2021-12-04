using System;
using System.Threading;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

public abstract class Command : CommandBase
{
    public abstract void Handle(CancellationToken ct);

    public override void Process(string command, string[] args, CancellationToken ct)
    {
        var root = Root!;
        if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description));
            return;
        }

        Handle(ct);
    }
}