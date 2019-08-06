using System;
using System.Linq;
using System.Reflection;
using Annium.Testing.Elements;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Annium.Testing.TestAdapter
{
    public class TestConverter
    {
        private readonly Uri executorUri;

        public TestConverter(string executorUri)
        {
            this.executorUri = new Uri(executorUri);
        }

        public TestCase Convert(Assembly assembly, Test test)
        {
            var testCase = new TestCase();
            testCase.ExecutorUri = executorUri;
            testCase.Source = assembly.FullName;
            testCase.CodeFilePath = test.File;
            testCase.LineNumber = test.Line;
            testCase.FullyQualifiedName = test.FullyQualifiedName;
            testCase.DisplayName = test.DisplayName;

            return testCase;
        }

        public Test Convert(Assembly assembly, TestCase test)
        {
            var fqn = test.FullyQualifiedName.Split('.');
            var type = string.Join('.', fqn.SkipLast(1));
            var name = fqn[fqn.Length - 1];

            var method = assembly.GetType(type).GetMethod(name);

            return new Test(method);
        }
    }
}