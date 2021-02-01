using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public static class AwaitableExtensions
    {
        public static void Await(this Task task) => task.GetAwaiter().GetResult();
        public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}