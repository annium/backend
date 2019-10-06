using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Requests;
using Demo.Core.Mediator.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.Mediator
{
    public class Program
    {
        private static async Task RunAsync(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var mediator = provider.GetRequiredService<IMediator>();
            _ = provider.GetRequiredService<TodoRepository>();

            var options = new JsonSerializerOptions().ConfigureForOperations();

            var request = new CreateTodoRequest("wake up");
            var payload = encode(request);
            var result = await mediator.SendAsync<Request<CreateTodoRequest>, Response<IBooleanResult<int>>>(payload, token);
            _ = decode(result);

            /*
            emulation:
            - logging handler (no mutation)             Request -> Request
            - conversion handler (mutates both):        Request -> TRequest
            - exception handler (mutates response)      TResponse -> Result
            - validation handler (no mutation)          TRequest -> TResponse (or throws ValidationException)
            - authorization handler (mutates request)   TRequest -> Authorized<TRequest>
            - command handler (returns response)        TRequest -> TResponse
            - response handler (mutates response)
             */

            Request<TRequest> encode<TRequest>(TRequest e) => new Request<TRequest>(JsonSerializer.Serialize(e, options));

            TResponse decode<TResponse>(Response<TResponse> e) => JsonSerializer.Deserialize<TResponse>(e.Value, options);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .RunAsync(RunAsync, args);
    }
}