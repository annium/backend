using System.Reflection;

namespace Annium.Core.Primitives.Reflection
{
    public static class AssemblyNameExtensions
    {
        public static string FriendlyName(this AssemblyName name) => $"{name.Name}:{name.Version}";
    }
}