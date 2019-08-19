using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Mediator.Internal
{
    internal class NextBuilder
    {
        private readonly MethodInfo executeAsync = typeof(ChainExecutor)
            .GetMethod(nameof(ChainExecutor.ExecuteAsync), BindingFlags.Public | BindingFlags.Static);

        private readonly MethodInfo getAwaiter = typeof(Task<object>).GetMethod(nameof(Task<int>.GetAwaiter));

        private readonly MethodInfo getResult = typeof(TaskAwaiter<object>).GetMethod(nameof(TaskAwaiter<int>.GetResult));

        private readonly MethodInfo fromResult = typeof(Task).GetMethod(nameof(Task.FromResult));

        public NextPrototype BuildNextPrototype(
            Type input,
            Type output
        ) => new NextPrototype(
            executeAsync,
            Expression.Parameter(input),
            getAwaiter,
            getResult,
            output,
            fromResult.MakeGenericMethod(output)
        );
    }
}