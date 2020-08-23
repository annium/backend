using System.Reflection;

namespace Annium.Extensions.Primitives
{
    public static class AssemblyNameExtensions
    {
        public static string FriendlyName(this AssemblyName name) => $"{name.Name}:{name.Version}";
    }
}