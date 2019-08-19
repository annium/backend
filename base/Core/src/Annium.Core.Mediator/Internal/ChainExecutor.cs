using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal static class ChainExecutor
    {
        public static async Task<object> ExecuteAsync(
            IServiceProvider provider,
            IReadOnlyList<ChainElement> chain,
            object request,
            CancellationToken cancellationToken,
            int index = 0
        )
        {
            var isFinal = index < chain.Count() - 1;
            var element = chain[index];

            var parameters = new List<object> { request, cancellationToken };
            if (isFinal)
                parameters.Add(element.Next.Compile(provider, chain, cancellationToken, index + 1));

            var handler = element.Handler;
            var handleMethodName = isFinal ? Constants.FinalHandlerHandleAsyncName : Constants.PipeHandlerHandleAsyncName;
            var handleMethod = handler.GetMethod(handleMethodName, parameters.Select(p => p.GetType()).ToArray());
            var result = handleMethod.Invoke(provider.GetRequiredService(handler), parameters.ToArray());
            await ((Task) result);

            return result.GetType().GetProperty(Constants.TaskResultName)
                .GetGetMethod().Invoke(result, Array.Empty<object>());
        }
    }
}