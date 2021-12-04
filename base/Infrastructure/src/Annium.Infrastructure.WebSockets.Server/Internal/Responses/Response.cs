using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Responses;

internal static class Response
{
    public static ResultResponse Result(Guid id, IStatusResult<OperationStatus> result) =>
        new(id, result);

    public static ResultResponse<T> Result<T>(Guid id, IStatusResult<OperationStatus, T> result) =>
        new(id, result);

    public static StreamHeadResponse<T> Head<T>(Guid id, IStatusResult<OperationStatus, T> result) =>
        new(id, result);

    public static StreamChunkResponse<T> Chunk<T>(Guid id, T data) =>
        new(id, data);

    public static StreamEndResponse End(Guid id) =>
        new(id);

    public static VoidResponse Void() =>
        new();

    public static VoidResponse<T> Void<T>() =>
        new();

    public static VoidResponse<T1, T2> Void<T1, T2>() =>
        new();

    public static MetaResponse<T, TR> Meta<T, TR>(TR response)
        where TR : AbstractResponseBase =>
        new(response);

    public static MetaResponse<T1, T2, TR> Meta<T1, T2, TR>(TR response)
        where TR : AbstractResponseBase =>
        new(response);
}