using System.IO;
using Annium.Core.Runtime.Resources;

namespace Annium.Core.Runtime.Internal.Resources
{
    internal class Resource : IResource
    {
        public string Name { get; }
        public Stream Content { get; }

        public Resource(
            string name,
            Stream content
        )
        {
            Name = name;
            Content = content;
        }

        public void Deconstruct(
            out string name,
            out Stream content
        )
        {
            name = Name;
            content = Content;
        }
    }
}