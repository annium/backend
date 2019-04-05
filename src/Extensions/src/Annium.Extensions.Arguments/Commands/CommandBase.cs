using System.Threading;

namespace Annium.Extensions.Arguments
{
    public abstract class CommandBase
    {
        public abstract string Id { get; }

        public abstract string Description { get; }

        internal Root Root { get; set; }

        public abstract void Process(string command, string[] args, CancellationToken token);
    }
}