using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ex = System.Linq.Expressions.Expression;

namespace Annium.Core.Mediator.Internal
{
    internal class NextBuilder
    {
        private readonly MethodInfo executeAsync = typeof(ChainExecutor)
            .GetMethod(nameof(ChainExecutor.ExecuteAsync), BindingFlags.Public | BindingFlags.Static);

        private readonly MethodInfo getAwaiter = typeof(Task<object>).GetMethod(nameof(Task<int>.GetAwaiter));

        private readonly MethodInfo getResult = typeof(TaskAwaiter<object>).GetMethod(nameof(TaskAwaiter<int>.GetResult));

        private readonly MethodInfo fromResult = typeof(Task).GetMethod(nameof(Task.FromResult));

        public Delegate BuildNext(
            Type input,
            Type output
        )
        {
            var provider = Ex.Parameter(typeof(IServiceProvider));
            var chain = Ex.Parameter(typeof(IReadOnlyList<ChainElement>));
            var index = Ex.Parameter(typeof(int));
            var token = Ex.Parameter(typeof(CancellationToken));
            var request = Ex.Parameter(input);

            // get next lambda, as awaiting call to next
            var next = Ex.Lambda(
                Ex.Call(
                    null,
                    fromResult.MakeGenericMethod(output),
                    Ex.Convert(
                        Ex.Call(
                            Ex.Call(
                                Ex.Call(null, executeAsync, provider, chain, Ex.Convert(request, typeof(object)), token, index),
                                getAwaiter
                            ),
                            getResult
                        ),
                        output
                    )
                ),
                request
            );

            return Ex.Lambda(next, provider, chain, token, index).Compile();
        }
    }
}