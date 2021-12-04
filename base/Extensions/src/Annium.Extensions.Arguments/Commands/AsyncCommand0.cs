using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

public abstract class AsyncCommand : CommandBase
{
    public abstract Task HandleAsync(CancellationToken ct);

    public override void Process(string command, string[] args, CancellationToken ct)
    {
        var root = Root!;
        if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
        {
            Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description));
            return;
        }

        HandleAsync(ct).Await();
    }
}