using System;
using Annium.Testing.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing.Executors
{
    public class Target
    {
        public object Instance { get; private set; }

        public Test Test { get; }

        public TestResult Result { get; }

        private readonly IServiceScope scope;

        public Target(
            IServiceScope scope,
            Test test,
            TestResult result
        )
        {
            Test = test;
            Result = result;
            this.scope = scope;
        }

        public void Init()
        {
            if (Instance == null)
                Instance = scope.ServiceProvider.GetRequiredService(Test.Method.DeclaringType);
            else
                throw new InvalidOperationException("Instance already created");
        }

        public void Deconstruct(
            out object instance,
            out Test test,
            out TestResult result
        )
        {
            instance = Instance;
            test = Test;
            result = Result;
        }
    }
}