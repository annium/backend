using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    // request -> void
    public interface IRequestHandler<TRequest> :
        IFinalRequestHandler<IRequestContext<TRequest>, IStatusResult<OperationStatus>>
        where TRequest : RequestBase
    {
    }

    // request -> response
    public interface IRequestResponseHandler<TRequest, TResponse> :
        IFinalRequestHandler<IRequestContext<TRequest>, IStatusResult<OperationStatus, TResponse>>
        where TRequest : RequestBase
    {
    }
    //
    // // request -> response stream
    // public interface IRequestResponseStreamHandler<TRequest, TResponseChunk> :
    //     IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, DataStream<TResponseChunk>>>
    //     where TRequest : RequestBase
    // {
    // }
    //
    // // request -> response stream
    // public interface IRequestResponseHeadedStreamHandler<TRequest, TResponse, TResponseChunk> :
    //     IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, DataStream<TResponse, TResponseChunk>>>
    //     where TRequest : RequestBase
    // {
    // }
    //
    // // request stream -> void
    // public interface IRequestStreamHandler<TRequestChunk> :
    //     IFinalRequestHandler<DataStream<TRequestChunk>, IStatusResult<OperationStatus>>
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> void
    // public interface IRequestHeadedStreamHandler<TRequest, TRequestChunk> :
    //     IFinalRequestHandler<DataStream<TRequest, TRequestChunk>, IStatusResult<OperationStatus>>
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response
    // public interface IRequestStreamResponseHandler<TRequestChunk, TResponse> :
    //     IFinalRequestHandler<DataStream<TRequestChunk>, IStatusResult<OperationStatus, TResponse>>
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response
    // public interface IRequestHeadedStreamResponseHandler<TRequest, TRequestChunk, TResponse> :
    //     IFinalRequestHandler<DataStream<TRequest, TRequestChunk>, IStatusResult<OperationStatus, TResponse>>
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response stream
    // public interface IRequestStreamResponseStreamHandler<TRequestChunk, TResponseChunk> :
    //     IFinalRequestHandler<DataStream<TRequestChunk>, IStatusResult<OperationStatus, DataStream<TResponseChunk>>>
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response stream
    // public interface IRequestStreamResponseHeadedStreamHandler<TRequestChunk, TResponse, TResponseChunk> :
    //     IFinalRequestHandler<DataStream<TRequestChunk>, IStatusResult<OperationStatus, DataStream<TResponse, TResponseChunk>>>
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response stream
    // public interface IRequestHeadedStreamResponseStreamHandler<TRequest, TRequestChunk, TResponseChunk> :
    //     IFinalRequestHandler<DataStream<TRequest, TRequestChunk>, IStatusResult<OperationStatus, DataStream<TResponseChunk>>>
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
    //
    // // request stream -> response stream
    // public interface IRequestHeadedStreamResponseHeadedStreamHandler<TRequest, TRequestChunk, TResponse, TResponseChunk> :
    //     IFinalRequestHandler<DataStream<TRequest, TRequestChunk>, IStatusResult<OperationStatus, DataStream<TResponse, TResponseChunk>>>
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase
    // {
    // }
}