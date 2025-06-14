// using System.Threading;
// using System.Threading.Tasks;
// // // using Annium.Mesh.Tests.System.Domain;
//
// namespace Annium.Mesh.Tests.System.Server.Demo;
//
// internal class AnalyticsHandler : IEventHandler<AnalyticEvent>
// {
//     private readonly SharedDataContainer _container;
//
//     public AnalyticsHandler(SharedDataContainer container)
//     {
//         _container = container;
//     }
//
//     public Task<None> HandleAsync(IRequestContext<AnalyticEvent> request, CancellationToken ct)
//     {
//         _container.Log.Enqueue(request.Request.Message);
//
//         return Task.FromResult(None.Default);
//     }
// }
