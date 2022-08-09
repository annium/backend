using System.Reflection;

namespace Annium.Core.Reflection.Extensions;

internal static class Constants
{
    public static readonly BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
}