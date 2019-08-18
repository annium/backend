namespace Annium.Data.Operations
{
    public sealed class StatusResult<S, D> : ResultBase<StatusResult<S, D>>, IStatusResult<StatusResult<S, D>, S, D>
    {
        public S Status { get; }

        public D Data { get; }

        internal StatusResult(S status, D data)
        {
            Status = status;
            Data = data;
        }

        public void Deconstruct(out S status, out D data)
        {
            status = Status;
            data = Data;
        }
    }

    public sealed class StatusResult<S> : ResultBase<StatusResult<S>>, IStatusResult<StatusResult<S>, S>
    {
        public S Status { get; }

        internal StatusResult(S status)
        {
            Status = status;
        }
    }
}