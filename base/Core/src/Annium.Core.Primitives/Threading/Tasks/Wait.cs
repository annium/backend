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
            try
            {
                while (condition())
                    await Task.Delay(pollDelay, ct).ConfigureAwait(true);
            }
            catch (TaskCanceledException)
            {
                // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
                // In this case, we only want to stop polling and finish this async Task.
            }
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
            try
            {
                while (!condition())
                    await Task.Delay(pollDelay, ct).ConfigureAwait(true);
            }
            catch (TaskCanceledException)
            {
                // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
                // In this case, we only want to stop polling and finish this async Task.
            }
        }
    }
}