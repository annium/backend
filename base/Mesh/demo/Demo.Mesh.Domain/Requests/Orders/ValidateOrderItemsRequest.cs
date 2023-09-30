using System;
using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request -> response stream
public class ValidateOrderItemsRequest : RequestBase
{
    public Guid Id { get; init; }
}