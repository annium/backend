using System;
using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request -> response
public class CreateOrderRequest : RequestBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}