using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Primitives;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.AspNetCore.TestServer.Handlers.Demo
{
    internal class SecondSubscriptionHandler :
        ISubscriptionHandler<SecondSubscriptionInit, string, ConnectionState>
    {
        private readonly SharedDataContainer _container;

        public SecondSubscriptionHandler(
            SharedDataContainer container
        )
        {
            _container = container;
        }

        public async Task<Unit> HandleAsync(
            ISubscriptionContext<SecondSubscriptionInit, string, ConnectionState> ctx,
            CancellationToken ct
        )
        {
            _container.Log.Add($"second init: {ctx.Request.Param}");
            ctx.Handle(Result.Status(OperationStatus.Ok));

            _container.Log.Add("second msg");
            ctx.Send("second msg");

            await ct;

            _container.Log.Add("second canceled");

            return Unit.Default;
        }
    }
}