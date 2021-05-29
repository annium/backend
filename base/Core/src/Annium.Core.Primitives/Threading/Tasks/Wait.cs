using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public static class Wait
    {
        /// <summary>
        ///     Blocks while condition is true or task is canceled.
        /// </summary>
        /// <param name="condition">
        ///     The condition that will perpetuate the block.
        /// </param>
        /// <param name="ct">
        ///     Cancellation token.
        /// </param>
        /// <param name="pollDelay">
        ///     The delay at which the condition will be polled, in milliseconds.
        /// </param>
        /// <returns>
        ///     <see cref="Task" />.
        /// </returns>
        public static async Task WhileAsync(Func<bool> condition, CancellationToken ct = default, int pollDelay = 25)
        {
            while (condition() && !ct.IsCancellationRequested)
                await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
        }

        /// <summary>
        ///     Blocks until condition is true or task is canceled.
        /// </summary>
        /// <param name="condition">
        ///     The condition that will perpetuate the block.
        /// </param>
        /// <param name="ct">
        ///     Cancellation token.
        /// </param>
        /// <param name="pollDelay">
        ///     The delay at which the condition will be polled, in milliseconds.
        /// </param>
        /// <returns>
        ///     <see cref="Task" />.
        /// </returns>
        public static async Task UntilAsync(Func<bool> condition, CancellationToken ct = default, int pollDelay = 25)
        {
            while (!condition() && !ct.IsCancellationRequested)
                await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
        }
    }
}