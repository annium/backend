using System;
using Annium.Testing.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing.Executors
{
    public class Target
    {
        public Test Test { get; }
        public TestResult Result { get; }
        private readonly IServiceProvider provider;
        public object? instance;

        public Target(
            IServiceProvider provider,
            Test test,
            TestResult result
        )
        {
            Test = test;
            Result = result;
            this.provider = provider;
        }

        public void Init()
        {
            if (instance is null)
                instance = provider.GetRequiredService(Test.Method.DeclaringType);
            else
                throw new InvalidOperationException("Instance already created");
        }

        public void Deconstruct(
            out object instance,
            out Test test,
            out TestResult result
        )
        {
            instance = this.instance!;
            test = Test;
            result = Result;
        }
    }
}