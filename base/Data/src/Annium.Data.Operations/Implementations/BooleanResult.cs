namespace Annium.Data.Operations.Implementations
{
    internal sealed class BooleanResult<D> : ResultBase<IBooleanResult<D>>, IBooleanResult<D>
    {
        public bool IsSuccess => value;
        public bool IsFailure => !value;
        public D Data { get; }
        private readonly bool value;

        internal BooleanResult(bool value, D data)
        {
            this.value = value;
            Data = data;
        }

        public void Deconstruct(out bool succeed, out D data)
        {
            succeed = IsSuccess;
            data = Data;
        }

        public override IBooleanResult<D> Clone()
        {
            var clone = new BooleanResult<D>(value, Data);
            this.CloneTo(clone);

            return clone;
        }
    }

    internal sealed class BooleanResult : ResultBase<IBooleanResult>, IBooleanResult
    {
        public bool IsSuccess => value;
        public bool IsFailure => !value;
        private readonly bool value;

        internal BooleanResult(bool value)
        {
            this.value = value;
        }

        public override IBooleanResult Clone()
        {
            var clone = new BooleanResult(value);
            this.CloneTo(clone);

            return clone;
        }
    }
}