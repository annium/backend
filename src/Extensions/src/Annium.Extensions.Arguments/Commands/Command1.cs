using System;
using System.Threading;

namespace Annium.Extensions.Arguments
{
    public abstract class Command<T> : CommandBase
    where T : new()
    {
        public abstract void Handle(T cfg, CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description, typeof(T)));
                return;
            }

            var cfg = Root.ConfigurationBuilder.Build<T>(args);

            Handle(cfg, token);
        }
    }
}