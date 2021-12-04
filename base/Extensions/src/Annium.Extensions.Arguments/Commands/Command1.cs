using System;
using System.Threading;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

public abstract class Command<T> : CommandBase
    where T : new()
{
    public abstract void Handle(T cfg, CancellationToken ct);

    public override void Process(string command, string[] args, CancellationToken ct)
    {
        var root = Root!;
        if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description, typeof(T)));
            return;
        }

        var cfg = root.ConfigurationBuilder.Build<T>(args);

        Handle(cfg, ct);
    }
}