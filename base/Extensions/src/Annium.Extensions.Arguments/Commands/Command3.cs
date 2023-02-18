using System;
using System.Threading;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

public abstract class Command<T1, T2, T3> : CommandBase
    where T1 : new()
    where T2 : new()
    where T3 : new()
{
    public abstract void Handle(T1 cfg1, T2 cfg2, T3 cfg3, CancellationToken ct);

    public override void Process(string command, string[] args, CancellationToken ct)
    {
        var root = Root!;
        if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description, typeof(T1), typeof(T2), typeof(T3)));
            return;
        }

        var cfg1 = root.ConfigurationBuilder.Build<T1>(args);
        var cfg2 = root.ConfigurationBuilder.Build<T2>(args);
        var cfg3 = root.ConfigurationBuilder.Build<T3>(args);

        Handle(cfg1, cfg2, cfg3, ct);
    }
}