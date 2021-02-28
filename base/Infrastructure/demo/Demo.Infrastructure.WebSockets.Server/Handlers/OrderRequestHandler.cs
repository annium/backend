using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    internal class OrderRequestHandler :
            IRequestHandler<DeleteOrderRequest>,
            IRequestResponseHandler<CreateOrderRequest, int>
        // IRequestResponseStreamHandler<ValidateOrderItemsRequest, DataStream<bool>>,
        // IRequestStreamHandler<BulkDeleteOrderStream>,
        // IRequestHeadedStreamResponseHandler<UploadOrderFileRequest, UploadOrderFileStreamChunkRequest, Guid>,
        // IRequestHeadedStreamResponseHeadedStreamHandler<BulkCreateOrdersRequest, BulkCreateOrderStreamChunkRequest, int, Guid>
    {
        public async Task<IStatusResult<OperationStatus>> HandleAsync(
            IRequestContext<DeleteOrderRequest> ctx,
            CancellationToken ct
        )
        {
            await Task.CompletedTask;

            return Result.Status(OperationStatus.Ok);
        }

        public async Task<IStatusResult<OperationStatus, int>> HandleAsync(
            IRequestContext<CreateOrderRequest> ctx,
            CancellationToken ct
        )
        {
            var (_, state) = ctx;
            int value;
            using (var _ = state.Lock())
            {
                var key = "inc";
                if (state.TryGet(key, out value))
                    state.Set(key, value += 1);
                else
                    state.Set(key, value = 1);
            }

            await Task.CompletedTask;

            return Result.Status(OperationStatus.Ok, value);
        }

        //
        // public Task<IStatusResult<OperationStatus, DataStream<DataStream<bool>>>> HandleAsync(
        //     ValidateOrderItemsRequest request,
        //     CancellationToken ct
        // )
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus>> HandleAsync(
        //     DataStream<BulkDeleteOrderStream> request,
        //     CancellationToken ct
        // )
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, Guid>> HandleAsync(
        //     DataStream<UploadOrderFileRequest, UploadOrderFileStreamChunkRequest> request,
        //     CancellationToken ct
        // )
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<int, Guid>>> HandleAsync(
        //     DataStream<BulkCreateOrdersRequest, BulkCreateOrderStreamChunkRequest> request,
        //     CancellationToken ct
        // )
        // {
        //     throw new NotImplementedException();
        // }
    }
}