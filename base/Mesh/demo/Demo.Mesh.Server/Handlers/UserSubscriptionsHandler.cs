using System;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;
using Demo.Mesh.Domain.Requests.User;
using Demo.Mesh.Domain.Responses.User;

namespace Demo.Mesh.Server.Handlers;

internal class UserSubscriptionsHandler :
    ISubscriptionHandler<UserBalanceSubscriptionInit, UserBalanceMessage, ConnectionState>
{
    public async Task<None> HandleAsync(
        ISubscriptionContext<UserBalanceSubscriptionInit, UserBalanceMessage, ConnectionState> ctx,
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

        return None.Default;
    }
}