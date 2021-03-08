using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders
{
    // request -> void
    public class DeleteOrderRequest : RequestBase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}