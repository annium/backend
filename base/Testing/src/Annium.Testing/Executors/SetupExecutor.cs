using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class SetupExecutor : ITestExecutor, ILogSubject
    {
        public ILogger Logger { get; }

        public uint Order { get; } = 2;

        public SetupExecutor(
            ILogger<SetupExecutor> logger
        )
        {
            Logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            this.Trace($"Setup {target.Test.DisplayName}.");

            target.Init();

            return Task.CompletedTask;
        }
    }
}