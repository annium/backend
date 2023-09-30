using System;
using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request -> void
public class DeleteOrderRequest : RequestBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}