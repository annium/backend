using System.Collections.Generic;
using System.Linq;

namespace Annium.Configuration.Abstractions
{
    public abstract class ConfigurationProviderBase
    {
        protected Dictionary<string[], string> data = new Dictionary<string[], string>();

        protected Stack<string> context = new Stack<string>();

        protected string[] Path => context.Reverse().ToArray();

        public abstract IReadOnlyDictionary<string[], string> Read();

        protected void Init()
        {
            data = new Dictionary<string[], string>();
            context = new Stack<string>();
        }
    }
}