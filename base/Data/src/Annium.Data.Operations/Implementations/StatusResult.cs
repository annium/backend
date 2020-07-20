namespace Annium.Data.Operations.Implementations
{
    internal sealed class StatusResult<S, D> : ResultBase<IStatusResult<S, D>>, IStatusResult<S, D>
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

        public override IStatusResult<S, D> Clone()
        {
            var clone = new StatusResult<S, D>(Status, Data);
            CloneTo(clone);

            return clone;
        }
    }

    internal sealed class StatusResult<S> : ResultBase<IStatusResult<S>>, IStatusResult<S>
    {
        public S Status { get; }

        internal StatusResult(S status)
        {
            Status = status;
        }

        public override IStatusResult<S> Clone()
        {
            var clone = new StatusResult<S>(Status);
            CloneTo(clone);

            return clone;
        }
    }
}