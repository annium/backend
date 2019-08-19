using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Annium.Core.Mediator.Internal
{
    internal class NextPrototype
    {
        private readonly MethodInfo executeAsync;
        private readonly ParameterExpression request;
        private readonly MethodInfo getAwaiter;
        private readonly MethodInfo getResult;
        private readonly Type output;
        private readonly MethodInfo fromResult;

        public NextPrototype(
            MethodInfo executeAsync,
            ParameterExpression request,
            MethodInfo getAwaiter,
            MethodInfo getResult,
            Type output,
            MethodInfo fromResult
        )
        {
            this.executeAsync = executeAsync;
            this.request = request;
            this.getAwaiter = getAwaiter;
            this.getResult = getResult;
            this.output = output;
            this.fromResult = fromResult;
        }

        public Delegate Compile(
            IServiceProvider provider,
            IReadOnlyList<ChainElement> chain,
            CancellationToken cancellationToken,
            int index
        )
        {
            // next function, returns Task<object>
            var next = Expression.Call(
                null,
                executeAsync,
                Expression.Constant(provider),
                Expression.Constant(chain),
                request,
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
            var body = Expression.Call(null, fromResult, result);

            return Expression.Lambda(body, request).Compile();
        }
    }
}