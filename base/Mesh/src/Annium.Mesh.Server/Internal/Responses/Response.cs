using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Server.Internal.Responses;

internal static class Response
{
    public static Domain.Response Result(Guid id, IStatusResult<OperationStatus> result) =>
        new() { Id = id, Result = result };

    public static Domain.Response<T> Result<T>(Guid id, IStatusResult<OperationStatus, T> result) =>
        new() { Id = id, Result = result! };
}