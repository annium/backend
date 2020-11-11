namespace Annium.Data.Operations.Implementations
{
    internal sealed class Result<TD> : ResultBase<IResult<TD>>, IResult<TD>
    {
        public TD Data { get; }

        internal Result(TD data)
        {
            Data = data;
        }

        public override IResult<TD> Clone()
        {
            var clone = new Result<TD>(Data);
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