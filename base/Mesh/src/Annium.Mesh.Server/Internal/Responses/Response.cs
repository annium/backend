using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Internal.Responses;

internal static class Response
{
    public static ResultResponse Result(Guid id, IStatusResult<OperationStatus> result) =>
        new(id, result);

    public static ResultResponse<T> Result<T>(Guid id, IStatusResult<OperationStatus, T> result) =>
        new(id, result);

    public static VoidResponse<T> Void<T>() =>
        new();
}