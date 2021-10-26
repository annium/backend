using System;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Primitives.Threading.Tasks;

namespace Annium.Testing
{
    public static class DelegateExtensions
    {
        public static TException Throws<TException>(this Delegate value)
            where TException : Exception
            => value.ThrowsWith<TException>(e => e);

        public static TException ThrowsWith<TException>(this Delegate value, string message, params string[] messages)
            where TException : Exception
            => value.ThrowsWith<TException>(e => e.Reports(message, messages));

        public static TException ThrowsWithExactly<TException>(this Delegate value, string message)
            where TException : Exception
            => value.ThrowsWith<TException>(e => e.ReportsExactly(message));

        private static TException ThrowsWith<TException>(this Delegate value, Func<TException, TException> validate)
            where TException : Exception
        {
            try
            {
                var result = value.DynamicInvoke();
                if (result is Task task) task.Await();
            }
            catch (TargetInvocationException exception)
            {
                return validate(exception.InnerException!.Is<TException>());
            }

            throw new AssertionFailedException($"{typeof(TException).Name} was not thrown");
        }

        private static TException Is<TException>(this Exception value) where TException : Exception =>
            value.As<TException>($"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`");
    }
}