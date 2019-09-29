using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class MethodExecutor
    {
        private readonly ILogger logger;

        public MethodExecutor(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public async Task ExecuteAsync(object instance, MethodInfo method, TestResult result)
        {
            logger.LogTrace($"Start execution of {method.DeclaringType!.Name}.{method.Name}");

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                if (method.Invoke(instance, new object[] { }) is Task task)
                    await task;
            }
            catch (TargetInvocationException exception)
            {
                HandleException(method, result, exception.InnerException!);
            }
            catch (Exception exception)
            {
                HandleException(method, result, exception);
            }
            finally
            {
                watch.Stop();
                result.ExecutionDuration.Add(new TimeSpan(watch.ElapsedTicks));

                logger.LogTrace($"Finished execution of {method.DeclaringType!.Name}.{method.Name}");
            }
        }

        private void HandleException(MethodInfo method, TestResult result, Exception exception)
        {
            result.Outcome = TestOutcome.Failed;
            result.Failure = exception;

            logger.LogTrace($"Failed execution of {method.DeclaringType!.Name}.{method.Name}: {exception}");
        }
    }
}