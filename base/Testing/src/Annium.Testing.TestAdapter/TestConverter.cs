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
            var testCase = new TestCase
            {
                ExecutorUri = executorUri,
                Source = assembly.FullName,
                CodeFilePath = test.File,
                LineNumber = test.Line,
                FullyQualifiedName = test.FullyQualifiedName,
                DisplayName = test.DisplayName
            };

            return testCase;
        }

        public Test Convert(Assembly assembly, TestCase test)
        {
            var fqn = test.FullyQualifiedName.Split('.');
            var type = string.Join('.', fqn.SkipLast(1));
            var name = fqn[^1];

            var method = assembly.GetType(type)?.GetMethod(name) ??
                throw new InvalidOperationException($"Failed to resolve {type}.{name} method");

            return new Test(method);
        }
    }
}