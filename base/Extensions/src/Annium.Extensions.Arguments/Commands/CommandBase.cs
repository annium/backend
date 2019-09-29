using System.Threading;

namespace Annium.Extensions.Arguments
{
    public abstract class CommandBase
    {
        public abstract string Id { get; }

        public abstract string Description { get; }

        internal Root? Root { get; private set; }

        public abstract void Process(string command, string[] args, CancellationToken token);

        internal void SetRoot(Root root)
        {
            Root = root;
        }
    }
}