using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors
{
    public class MethodExecutor
    {
        private readonly ILogger<MethodExecutor> _logger;

        public MethodExecutor(
            ILogger<MethodExecutor> logger
        )
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(object instance, MethodInfo method, TestResult result)
        {
            _logger.Trace($"Start execution of {method.DeclaringType!.Name}.{method.Name}");

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

                _logger.Trace($"Finished execution of {method.DeclaringType!.Name}.{method.Name}");
            }
        }

        private void HandleException(MethodInfo method, TestResult result, Exception exception)
        {
            result.Outcome = TestOutcome.Failed;
            result.Failure = exception;

            _logger.Trace($"Failed execution of {method.DeclaringType!.Name}.{method.Name}: {exception}");
        }
    }
}