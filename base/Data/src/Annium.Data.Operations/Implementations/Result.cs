namespace Annium.Data.Operations.Implementations
{
    internal sealed class Result : ResultBase<IResult>, IResult
    {
        internal Result() { }

        public override IResult Clone()
        {
            var clone = new Result();
            this.CloneTo(clone);

            return clone;
        }
    }
}