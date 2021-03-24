using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using Demo.Infrastructure.WebSockets.Domain.Responses.User;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    public class UserSubscriptionsHandler :
        ISubscriptionHandler<UserBalanceSubscriptionInit, UserBalanceMessage>
    {
        public async Task<Unit> HandleAsync(
            ISubscriptionContext<UserBalanceSubscriptionInit, UserBalanceMessage> ctx,
            CancellationToken ct
        )
        {
            // confirm subscription
            ctx.Handle(Result.Status(OperationStatus.Ok));

            // subscription body
            var rnd = new Random();
            var balance = rnd.Next(0, 100);
            // callback on subscription end
            ct.Register(() => Console.WriteLine("Subscription cancelled"));
            // main code to run until subscription ended
            while (!ct.IsCancellationRequested)
            {
                ctx.Send(new UserBalanceMessage { Balance = balance += rnd.Next(-4, 5) });
                await Task.Delay(500);
            }

            return Unit.Default;
        }
    }
}