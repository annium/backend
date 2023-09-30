using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

// request -> response
public class CreateOrderRequest : RequestBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}