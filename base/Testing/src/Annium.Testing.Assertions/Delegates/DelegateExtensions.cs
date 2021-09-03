using System;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading.Tasks;

namespace Annium.Testing
{
    public static class DelegateExtensions
    {
        public static TException Throws<TException>(this Delegate value) where TException : Exception
        {
            try
            {
                var result = value.DynamicInvoke();
                if (result is Task task) task.Await();
            }
            catch (TargetInvocationException exception)
            {
                return exception.InnerException!.Is<TException>();
            }

            throw new AssertionFailedException($"{typeof(TException).Name} was not thrown");
        }

        internal static TException Is<TException>(this Exception value) where TException : Exception =>
            value.As<TException>($"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`");
    }
}