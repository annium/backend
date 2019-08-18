using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Application.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal class Mediator : IMediator
    {
        private readonly MediatorConfiguration configuration;
        private readonly IServiceProvider provider;

        public Mediator(
            MediatorConfiguration configuration,
            IServiceProvider provider
        )
        {
            this.configuration = configuration;
            this.provider = provider;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = GetExecutionChain(typeof(TRequest), typeof(TResponse));

            // execute chain
            return (TResponse) await ExecuteAsync(chain, request, cancellationToken);
        }

        private async Task<object> ExecuteAsync(
            IReadOnlyList<Type> chain,
            object request,
            CancellationToken cancellationToken,
            int index = 0
        )
        {
            var isFinal = index < chain.Count() - 1;
            var handler = chain[index];

            var parameters = new List<object> { request, cancellationToken };
            if (isFinal)
                parameters.Add(BuildNext(handler, chain, cancellationToken, index + 1));

            var handleMethodName = isFinal ? Constants.FinalHandlerHandleAsyncName : Constants.PipeHandlerHandleAsyncName;
            var handleMethod = handler.GetMethod(handleMethodName, parameters.Select(p => p.GetType()).ToArray());
            var result = handleMethod.Invoke(provider.GetRequiredService(handler), parameters.ToArray());
            await ((Task) result);

            return result.GetType().GetProperty(Constants.TaskResultName)
                .GetGetMethod().Invoke(result, Array.Empty<object>());
        }

        private Delegate BuildNext(
            Type handler,
            IReadOnlyList<Type> chain,
            CancellationToken cancellationToken,
            int index
        )
        {
            var nextTypes = handler.GetTargetImplementation(typeof(IRequestHandlerOutput<,>)).GetGenericArguments();
            var input = nextTypes[0];
            var output = nextTypes[1];

            // single parameter, that will be passed by handler to next function
            var requestParameter = Expression.Parameter(input);

            // next function, returns Task<object>
            var next = Expression.Call(
                Expression.Constant(this),
                this.GetType().GetMethod(nameof(ExecuteAsync), BindingFlags.NonPublic | BindingFlags.Instance),
                Expression.Constant(chain),
                requestParameter,
                Expression.Constant(cancellationToken),
                Expression.Constant(index)
            );

            // get task awaiter
            var getAwaiter = typeof(Task<object>).GetMethod(nameof(Task<int>.GetAwaiter));
            var awaiter = Expression.Call(next, getAwaiter);

            // get awaiter result
            var getResult = typeof(TaskAwaiter<object>).GetMethod(nameof(TaskAwaiter<int>.GetResult));
            var resultObject = Expression.Call(awaiter, getResult);

            // convert to real output type
            var result = Expression.Convert(resultObject, output);

            // wrap to task
            var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(output);
            var body = Expression.Call(null, fromResult, result);

            return Expression.Lambda(body, requestParameter).Compile();
        }

        private IReadOnlyList<Type> GetExecutionChain(Type input, Type output)
        {
            var handlers = configuration.Handlers.ToList();
            var chain = new List<Type>();
            var isFinalized = false;

            while (true)
            {
                Type service = null;

                foreach (var handler in handlers.ToArray())
                {
                    service = resolveHandler(handler);
                    if (service is null)
                        continue;

                    handlers.Remove(handler);
                    break;
                }

                if (service is null)
                    break;

                chain.Add(service);

                var serviceOutput = service.GetTargetImplementation(Constants.HandlerOutputType);
                // if final handler - break
                if (serviceOutput is null)
                {
                    isFinalized = true;
                    break;
                }

                var outputArgs = serviceOutput.GetGenericArguments();
                input = outputArgs[0];
                output = outputArgs[1];
            }

            if (!isFinalized)
                throw new InvalidOperationException($"Can't resolve request handler by input {input} and output {output}");

            return chain;

            Type resolveHandler(Handler handler)
            {
                var requestIn = input.GetTargetImplementation(handler.RequestIn);
                var responseOut = handler.ResponseOut.ResolveByImplentations(output);
                // var responseOut = output.GetTargetImplementation(handler.ResponseOut);

                if (requestIn is null || responseOut is null)
                    return null;

                var handlerInput = typeof(IRequestHandlerInput<,>).MakeGenericType(requestIn, responseOut);
                var service = handler.Implementation.ResolveByImplentations(handlerInput);
                if (service is null)
                    throw new InvalidOperationException($"Can't resolve {handler.Implementation} by input {requestIn} and output {responseOut}");

                return service;
            }
        }
    }
}