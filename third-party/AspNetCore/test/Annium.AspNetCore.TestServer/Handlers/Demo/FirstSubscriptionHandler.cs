using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Primitives.Threading;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.AspNetCore.TestServer.Handlers.Demo
{
    internal class FirstSubscriptionHandler :
        ISubscriptionHandler<FirstSubscriptionInit, string, ConnectionState>
    {
        private readonly SharedDataContainer _container;

        public FirstSubscriptionHandler(
            SharedDataContainer container
        )
        {
            _container = container;
        }

        public async Task<Unit> HandleAsync(
            ISubscriptionContext<FirstSubscriptionInit, string, ConnectionState> ctx,
            CancellationToken ct
        )
        {
            _container.Log.Enqueue($"first init: {ctx.Request.Param}");
            ctx.Handle(Result.Status(OperationStatus.Ok));

            _container.Log.Enqueue("first msg1");
            ctx.Send("first msg1");

            _container.Log.Enqueue("first msg2");
            ctx.Send("first msg2");

            await ct;

            _container.Log.Enqueue("first canceled");

            return Unit.Default;
        }
    }
}