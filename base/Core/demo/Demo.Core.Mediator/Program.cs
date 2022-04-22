using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Demo.Core.Mediator;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Requests;
using Demo.Core.Mediator.ViewModels;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

var mediator = provider.Resolve<IMediator>();
_ = provider.Resolve<TodoRepository>();

var options = new JsonSerializerOptions().ConfigureForOperations();

var request = new CreateTodoRequest("wake up");
var payload = Encode(request);
var result = await mediator.SendAsync<Response<IBooleanResult<int>>>(payload, ct);
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