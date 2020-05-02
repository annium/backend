using System.IO;

namespace Annium.Core.Runtime.Resources
{
    public interface IResource
    {
        string Name { get; }
        Stream Content { get; }

        void Deconstruct(
            out string name,
            out Stream content
        );
    }
}