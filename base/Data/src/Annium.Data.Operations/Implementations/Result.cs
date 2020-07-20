namespace Annium.Data.Operations.Implementations
{
    internal sealed class Result<D> : ResultBase<IResult<D>>, IResult<D>
    {
        public D Data { get; }

        internal Result(D data)
        {
            Data = data;
        }

        public override IResult<D> Clone()
        {
            var clone = new Result<D>(Data);
            CloneTo(clone);

            return clone;
        }
    }

    internal sealed class Result : ResultBase<IResult>, IResult
    {
        public override IResult Clone()
        {
            var clone = new Result();
            CloneTo(clone);

            return clone;
        }
    }
}