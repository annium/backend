using System.Reflection;

namespace Annium.Extensions.Primitives
{
    public static class AssemblyExtensions
    {
        public static string FriendlyName(this Assembly assembly) => assembly.GetName().FriendlyName();
    }
}