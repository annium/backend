using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators
{
    public class TrackCompletionTest
    {
        [Fact]
        public async Task TrackCompletion_IncompleteWorks()
        {
            // arrange
            var cts = new CancellationTokenSource();
            var observable = ObservableExt.StaticAsyncInstance<string>(async ctx =>
            {
                await Task.Delay(10, ctx.Ct);

                return () => Task.CompletedTask;
            }, cts.Token).TrackCompletion();

            // act
            await observable.WhenCompleted();
        }

        [Fact]
        public async Task TrackCompletion_CompleteWorks()
        {
            // arrange
            var cts = new CancellationTokenSource();
            var observable = ObservableExt.StaticAsyncInstance<string>(_ => Task.FromResult<Func<Task>>(() => Task.CompletedTask), cts.Token).TrackCompletion();

            // act
            await observable.WhenCompleted();
        }
    }
}