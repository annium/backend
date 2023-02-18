using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Debug.Internal;

internal class Scheduler
{
    private readonly BlockingCollection<Delegate> _tasks = new();

    public void Schedule(Action work) => ScheduleWork(work);
    public void Schedule(Func<Task> work) => ScheduleWork(work);

    public Scheduler()
    {
        Task.Run(Run, CancellationToken.None);
    }

    private async Task Run()
    {
        while (true)
        {
            var task = _tasks.Take();
            switch (task)
            {
                case Action syncTask:
                    syncTask();
                    break;
                case Func<Task> asyncTask:
                    await asyncTask();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScheduleWork(Delegate work) => _tasks.Add(work);
}