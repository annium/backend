using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public static class Defer
    {
        /// <summary>
        ///     Executes some code asynchronously within given amount of time.
        /// </summary>
        /// <param name="handle">
        ///     Code to execute when time comes.
        /// </param>
        /// <param name="timeout">
        ///     The timeout at which the code will be run, in milliseconds.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action SetTimeout(Action handle, int timeout) => SetTimeout(handle, TimeSpan.FromMilliseconds(timeout));

        /// <summary>
        ///     Executes some code asynchronously within given amount of time.
        /// </summary>
        /// <param name="handle">
        ///     Code to execute when time comes.
        /// </param>
        /// <param name="timeout">
        ///     The timeout at which the code will be run, as <see cref="TimeSpan"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action SetTimeout(Action handle, TimeSpan timeout)
        {
            var execute = true;
            Task.Delay(timeout).ContinueWith(_ => handle());
            return () => execute = false;
        }

        /// <summary>
        ///     Executes some code asynchronously within given amount of time.
        /// </summary>
        /// <param name="handle">
        ///     Code to execute when time comes.
        /// </param>
        /// <param name="interval">
        ///     The interval at which the code will be run, as <see cref="TimeSpan"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action SetInterval(Action handle, TimeSpan interval)
        {
            var timer = new Timer(_ => handle(), null, interval, interval);

            return () => timer.Dispose();
        }
    }
}