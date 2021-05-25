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
            _container.Log.Add($"first init: {ctx.Request.Param}");
            ctx.Handle(Result.Status(OperationStatus.Ok));

            _container.Log.Add("first msg");
            ctx.Send("first msg");

            await ct;

            _container.Log.Add("first canceled");

            return Unit.Default;
        }
    }
}