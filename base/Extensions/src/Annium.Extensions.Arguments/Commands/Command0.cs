using System;
using System.Threading;

namespace Annium.Extensions.Arguments
{
    public abstract class Command : CommandBase
    {
        public abstract void Handle(CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            var root = Root!;
            if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description));
                return;
            }

            Handle(token);
        }
    }
}