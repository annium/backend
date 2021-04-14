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
namespace Demo.Core.Mediator
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var mediator = provider.Resolve<IMediator>();
            _ = provider.Resolve<TodoRepository>();

            var options = new JsonSerializerOptions().ConfigureForOperations();

            var request = new CreateTodoRequest("wake up");
            var payload = Encode(request);
            var result = await mediator.SendAsync<Response<IBooleanResult<int>>>(payload, token);
            _ = Decode(result);

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

            Request<TRequest> Encode<TRequest>(TRequest e) => new(JsonSerializer.Serialize(e, options));

            TResponse Decode<TResponse>(Response<TResponse> e) => JsonSerializer.Deserialize<TResponse>(e.Value, options)!;
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}