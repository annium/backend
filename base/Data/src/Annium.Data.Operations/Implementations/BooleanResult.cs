namespace Annium.Data.Operations.Implementations
{
    internal sealed class BooleanResult<D> : ResultBase<IBooleanResult<D>>, IBooleanResult<D>
    {
        public bool IsSuccess => _value;
        public bool IsFailure => !_value;
        public D Data { get; }
        private readonly bool _value;

        internal BooleanResult(bool value, D data)
        {
            _value = value;
            Data = data;
        }

        public void Deconstruct(out bool succeed, out D data)
        {
            succeed = IsSuccess;
            data = Data;
        }

        public override IBooleanResult<D> Clone()
        {
            var clone = new BooleanResult<D>(_value, Data);
            CloneTo(clone);

            return clone;
        }
    }

    internal sealed class BooleanResult : ResultBase<IBooleanResult>, IBooleanResult
    {
        public bool IsSuccess => _value;
        public bool IsFailure => !_value;
        private readonly bool _value;

        internal BooleanResult(bool value)
        {
            _value = value;
        }

        public override IBooleanResult Clone()
        {
            var clone = new BooleanResult(_value);
            CloneTo(clone);

            return clone;
        }
    }
}