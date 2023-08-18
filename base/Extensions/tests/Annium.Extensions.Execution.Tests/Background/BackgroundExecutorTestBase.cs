using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Testing;

namespace Annium.Extensions.Execution.Tests.Background;

public abstract class BackgroundExecutorTestBase
{
    protected async Task Schedule_SyncAction_Base(IBackgroundExecutor executor)
    {
        Log.SetTestMode();
        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        executor.Schedule(() => success = true);

        // run and dispose executor
        executor.Start(cts.Token);
        await executor.DisposeAsync();

        // assert
        success.IsTrue();
    }

    protected async Task Schedule_SyncCancellableAction_Base(IBackgroundExecutor executor)
    {
        Log.SetTestMode();
        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        executor.Schedule(ct => ct.Register(() => isCancelled = true));

        // run and dispose executor
        executor.Start(cts.Token);
        await executor.DisposeAsync();

        // assert
        isCancelled.IsTrue();
    }

    protected async Task Schedule_AsyncAction_Base(IBackgroundExecutor executor)
    {
        Log.SetTestMode();
        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        executor.Schedule(async () =>
        {
            await Task.Delay(50, CancellationToken.None);
            success = true;
        });

        // run and dispose executor
        executor.Start(cts.Token);
        await executor.DisposeAsync();

        // assert
        success.IsTrue();
    }

    protected async Task Schedule_AsyncCancellableAction_Base(IBackgroundExecutor executor)
    {
        Log.SetTestMode();
        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        executor.Schedule(async ct =>
        {
            await Task.Delay(50, CancellationToken.None);
            ct.Register(() => isCancelled = true);
        });

        // run and dispose executor
        executor.Start(cts.Token);
        await executor.DisposeAsync();

        // assert
        isCancelled.IsTrue();
    }
}