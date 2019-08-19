using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator.Internal
{
    internal class NextBuilder
    {
        private readonly MethodInfo executeAsync = typeof(Mediator)
            .GetMethod(nameof(Mediator.ExecuteAsync), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly MethodInfo getAwaiter = typeof(Task<object>).GetMethod(nameof(Task<int>.GetAwaiter));

        private readonly MethodInfo getResult = typeof(TaskAwaiter<object>).GetMethod(nameof(TaskAwaiter<int>.GetResult));

        private readonly MethodInfo fromResult = typeof(Task).GetMethod(nameof(Task.FromResult));

        public Delegate BuildNext(
            IMediator mediator,
            ChainElement element,
            IReadOnlyList<ChainElement> chain,
            CancellationToken cancellationToken,
            int index
        )
        {
            var(input, output) = element.Output;

            // single parameter, that will be passed by handler to next function
            var requestParameter = Expression.Parameter(input);

            // next function, returns Task<object>
            var next = Expression.Call(
                Expression.Constant(mediator),
                executeAsync,
                Expression.Constant(chain),
                requestParameter,
                Expression.Constant(cancellationToken),
                Expression.Constant(index)
            );

            // get task awaiter
            var awaiter = Expression.Call(next, getAwaiter);

            // get awaiter result
            var resultObject = Expression.Call(awaiter, getResult);

            // convert to real output type
            var result = Expression.Convert(resultObject, output);

            // wrap to task
            var body = Expression.Call(null, fromResult.MakeGenericMethod(output), result);

            return Expression.Lambda(body, requestParameter).Compile();
        }
    }
}