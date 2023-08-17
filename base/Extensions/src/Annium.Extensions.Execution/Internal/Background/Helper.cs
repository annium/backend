using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal.Background;

internal static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task RunTaskInBackground(Delegate task) => task switch
    {
        Action execute          => Task.Run(execute),
        Func<ValueTask> execute => Task.Run(async () => await execute().ConfigureAwait(false)),
        Func<Task> execute      => Task.Run(async () => await execute().ConfigureAwait(false)),
        _                       => throw new NotSupportedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask RunTaskInForeground(Delegate task)
    {
        switch (task)
        {
            case Action t:
                t();
                break;
            case Func<ValueTask> t:
                await t();
                break;
            case Func<Task> t:
                await t();
                break;
            default:
                throw new NotSupportedException();
        }
    }
}