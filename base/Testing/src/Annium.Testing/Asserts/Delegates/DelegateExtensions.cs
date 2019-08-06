using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Testing
{
    public static class DelegateExtensions
    {
        public static TException Throws<TException>(this Delegate value) where TException : Exception
        {
            Exception ex = null;
            try
            {
                var result = value.DynamicInvoke();
                if (result is Task task) task.GetAwaiter().GetResult();
            }
            catch (TargetInvocationException exception)
            {
                ex = exception.InnerException;
            }

            return ex.Is<TException>();
        }

        internal static TException Is<TException>(this Exception value) where TException : Exception =>
            value.As<TException>($"Returned result is type `{value?.GetType()}`, not derived from expected type `{typeof(TException)}`");
    }
}