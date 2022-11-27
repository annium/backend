using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Demo.Extensions.Jobs;

internal class Job : ILogSubject<Job>
{
    public ILogger<Job> Logger { get; }
    private int _step;

    public Job(ILogger<Job> logger)
    {
        Logger = logger;
    }

    public async Task Execute()
    {
        var step = _step++;
        this.Log().Trace($"{step}: before");
        await Task.Delay(100);
        this.Log().Trace($"{step}: after");
    }
}