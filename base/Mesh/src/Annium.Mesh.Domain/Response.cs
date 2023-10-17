using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Domain;

public sealed record Response
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IStatusResult<OperationStatus> Result { get; init; } = Data.Operations.Result.Status(OperationStatus.Ok);
}

public sealed record Response<T>
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IStatusResult<OperationStatus, T?> Result { get; init; } = Data.Operations.Result.Status(OperationStatus.Ok, default(T));
}