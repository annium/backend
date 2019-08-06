using System.Collections.Generic;
using System.Linq;

namespace Annium.Configuration.Abstractions
{
    internal abstract class ConfigurationProviderBase
    {
        protected Dictionary<string[], string> data;

        protected Stack<string> context;

        protected string[] path => context.Reverse().ToArray();

        public abstract IReadOnlyDictionary<string[], string> Read();

        protected void Init()
        {
            data = new Dictionary<string[], string>();
            context = new Stack<string>();
        }
    }
}