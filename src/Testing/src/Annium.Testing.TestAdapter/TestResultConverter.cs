using System.Reflection;
using Annium.Testing.Elements;
using MsTestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;
using MsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Annium.Testing.TestAdapter
{
    public class TestResultConverter
    {
        private readonly TestConverter testConverter;

        public TestResultConverter(TestConverter testConverter)
        {
            this.testConverter = testConverter;
        }

        public MsTestResult Convert(Assembly assembly, Test test, TestResult testResult)
        {
            var result = new MsTestResult(testConverter.Convert(assembly, test));
            result.Outcome = (MsTestOutcome) testResult.Outcome;
            result.ErrorMessage = testResult.Failure?.Message;
            result.ErrorStackTrace = testResult.Failure?.StackTrace;
            result.DisplayName = test.DisplayName;
            result.Duration = testResult.ExecutionDuration;
            result.StartTime = testResult.ExecutionStart;
            result.EndTime = testResult.ExecutionEnd;

            return result;
        }
    }
}