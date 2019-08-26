namespace Annium.Data.Operations
{
    public sealed class BooleanResult<T> : ResultBase<BooleanResult<T>>, IBooleanResult<BooleanResult<T>, T>
    {
        public bool IsSuccess => value && !HasErrors;

        public bool IsFailure => !value || HasErrors;

        public T Data { get; }

        private readonly bool value;

        internal BooleanResult(bool value, T data)
        {
            this.value = value;
            Data = data;
        }

        public void Deconstruct(out bool succeed, out T data)
        {
            succeed = IsSuccess;
            data = Data;
        }

        public override BooleanResult<T> Clone()
        {
            var clone = new BooleanResult<T>(value, Data);
            this.CloneTo(clone);

            return clone;
        }
    }

    public sealed class BooleanResult : ResultBase<BooleanResult>, IBooleanResult<BooleanResult>
    {
        public bool IsSuccess => value && !HasErrors;

        public bool IsFailure => !value || HasErrors;

        private readonly bool value;

        internal BooleanResult(bool value)
        {
            this.value = value;
        }

        public override BooleanResult Clone()
        {
            var clone = new BooleanResult(value);
            this.CloneTo(clone);

            return clone;
        }
    }
}