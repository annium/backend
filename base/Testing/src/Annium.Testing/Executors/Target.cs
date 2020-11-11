using System;
using Annium.Testing.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing.Executors
{
    public class Target
    {
        public Test Test { get; }
        public TestResult Result { get; }
        private readonly IServiceProvider _provider;
        public object? Instance;

        public Target(
            IServiceProvider provider,
            Test test,
            TestResult result
        )
        {
            Test = test;
            Result = result;
            _provider = provider;
        }

        public void Init()
        {
            if (Instance is null)
                Instance = _provider.GetRequiredService(Test.Method.DeclaringType);
            else
                throw new InvalidOperationException("Instance already created");
        }

        public void Deconstruct(
            out object instance,
            out Test test,
            out TestResult result
        )
        {
            instance = Instance!;
            test = Test;
            result = Result;
        }
    }
}