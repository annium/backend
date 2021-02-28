using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders
{
    // request -> response stream
    public record ValidateOrderItemsRequest : RequestBase
    {
        public Guid Id { get; init; }
    }
}