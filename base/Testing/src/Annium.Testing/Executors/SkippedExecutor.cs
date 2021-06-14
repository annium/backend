﻿using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors
{
    public class SkippedExecutor : ITestExecutor, ILogSubject
    {
        public ILogger Logger { get; }

        public uint Order { get; } = 1;

        public SkippedExecutor(
            ILogger<SkippedExecutor> logger
        )
        {
            Logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            if (target.Test.IsSkipped)
            {
                target.Result.Outcome = TestOutcome.Skipped;

                this.Trace($"Skip {target.Test.DisplayName}.");
            }

            return Task.CompletedTask;
        }
    }
}